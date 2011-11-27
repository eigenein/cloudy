using System;
using System.Net;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Structures;
using Cloudy.Computing.Structures.Values;
using Cloudy.Helpers;
using Cloudy.Messaging.Interfaces;

namespace Cloudy.Computing
{
    public abstract class AbstractMasterNode : AbstractNode
    {
        private MasterState state;

        protected readonly INetworkRepository NetworkRepository;

        protected AbstractMasterNode(int port, INetworkRepository networkRepository) 
            : base(port)
        {
            this.NetworkRepository = networkRepository;
            AddHandler(Tags.JoinRequest, OnJoinRequest);
            AddHandler(Tags.Bye, OnBye);
            State = MasterState.Joined;
        }

        public MasterState State
        {
            get { return state; }
            private set 
            { 
                state = value;
                if (StateChanged != null)
                {
                    StateChanged(this, new EventArgs<MasterState>(state));
                }
            }
        }

        public event ParametrizedEventHandler<IPEndPoint, Guid> SlaveJoined;

        public event ParametrizedEventHandler<MasterState> StateChanged;

        public event ParametrizedEventHandler<IPEndPoint, Guid> SlaveLeft;

        public event ParametrizedEventHandler<JobResult> JobStopped;

        public event ParametrizedEventHandler<Guid, Guid> ThreadFailedToStart;

        public event EventHandler Started;

        public event EventHandler FailedToStart;

        public event ParametrizedEventHandler<Guid> StartingThread;

        private bool OnJoinRequest(IPEndPoint remoteEndPoint, IMessage message)
        {
            JoinRequestValue request = message.Get<JoinRequestValue>();
            Guid slaveId = request.SlaveId ?? Guid.NewGuid();
            JoinResponseValue response = new JoinResponseValue();
            response.ExternalEndPoint = new EndPointValue { Value = remoteEndPoint };
            response.SlaveId = slaveId;
            try
            {
                Send(remoteEndPoint, response, Tags.JoinResponse);
            }
            catch (TimeoutException)
            {
                SendAsync(remoteEndPoint, EmptyValue.Instance, Tags.Bye);
                return true;
            }
            SlaveContext slave;
            if (!NetworkRepository.TryGetSlave(slaveId, out slave))
            {
                NetworkRepository.AddToTotalSlotsCount(request.SlotsCount);
                slave = new SlaveContext();
                slave.SlaveId = slaveId;
                slave.SlotsCount = request.SlotsCount;
                slave.LocalEndPoint = request.LocalEndPoint.Value;
                slave.ExternalEndPoint = remoteEndPoint;
                NetworkRepository.AddSlave(remoteEndPoint, slave);
            }
            OnSlaveJoined(slave);
            if (SlaveJoined != null)
            {
                SlaveJoined(this, new EventArgs<IPEndPoint, Guid>(remoteEndPoint, slaveId));
            }
            return true;
        }

        protected abstract void OnSlaveJoined(SlaveContext slave);

        private bool OnBye(IPEndPoint remoteEndPoint, IMessage message)
        {
            SlaveContext slave = NetworkRepository.GetSlave(remoteEndPoint);
            NetworkRepository.RemoveFromTotalSlotsCount(slave.SlotsCount);
            NetworkRepository.RemoveSlave(remoteEndPoint);
            OnSlaveLeft(slave);
            if (SlaveLeft != null)
            {
                SlaveLeft(this, new EventArgs<IPEndPoint, Guid>(remoteEndPoint, slave.SlaveId));
            }
            return true;
        }

        protected abstract void OnSlaveLeft(SlaveContext slave);

        protected void CreateThreads(SlaveContext slave)
        {
            int count = slave.SlotsCount - NetworkRepository.GetThreadsCount(slave.SlaveId);
            while (count-- > 0)
            {
                ThreadContext thread = new ThreadContext();
                thread.State = ThreadState.NotRunning;
                thread.ThreadId = Guid.NewGuid();
                NetworkRepository.AddThread(slave.SlaveId, thread);
            }
        }

        protected abstract void OnThreadFailedToStart(Guid slaveId, Guid threadId);

        protected bool Start()
        {
            bool atLeastOneStarted = false;
            foreach (SlaveContext slave in NetworkRepository.GetAllSlaves())
            {
                foreach (ThreadContext thread in NetworkRepository.GetThreads(slave.SlaveId))
                {
                    if (StartingThread != null)
                    {
                        StartingThread(this, new EventArgs<Guid>(thread.ThreadId));
                    }
                    try
                    {
                        Send(slave.ExternalEndPoint, new GuidValue { Value = thread.ThreadId }, 
                            Tags.StartThread);
                        NetworkRepository.SetThreadState(thread.ThreadId, ThreadState.Running);
                        atLeastOneStarted = true;
                    }
                    catch (TimeoutException)
                    {
                        OnThreadFailedToStart(slave.SlaveId, thread.ThreadId);
                        if (ThreadFailedToStart != null)
                        {
                            ThreadFailedToStart(this, new EventArgs<Guid, Guid>(
                                slave.SlaveId, thread.ThreadId));
                        }
                    }
                }
            }
            if (atLeastOneStarted)
            {
                State = MasterState.Running;
                if (Started != null)
                {
                    Started(this, new EventArgs());
                }
            }
            else if (FailedToStart != null)
            {
                FailedToStart(this, new EventArgs());
            }
            return atLeastOneStarted;
        }

        protected abstract bool OnJobStopped(JobResult result);

        protected void StopJob(JobResult result)
        {
            StopAllThreads();
            State = MasterState.Joined;
            if (JobStopped != null)
            {
                JobStopped(this, new EventArgs<JobResult>(result));
            }
            if (OnJobStopped(result))
            {
                Start();
            }
            else
            {
                Close();
            }
        }

        private void StopAllThreads()
        {
            foreach (SlaveContext slave in NetworkRepository.GetAllSlaves())
            {
                foreach (ThreadContext thread in NetworkRepository.GetThreads(slave.SlaveId))
                {
                    if (thread.State == ThreadState.Running)
                    {
                        try
                        {
                            Send(slave.ExternalEndPoint, new GuidValue { Value = thread.ThreadId },
                                Tags.StopThread);
                            NetworkRepository.SetThreadState(thread.ThreadId, ThreadState.NotRunning);
                        }
                        catch (TimeoutException)
                        {
                            continue;
                        }
                    }
                }
            }
        }

        public void Close()
        {
            foreach (IPEndPoint endPoint in NetworkRepository.GetSlavesEndPoints())
            {
                SendAsync(endPoint, EmptyValue.Instance, Tags.Bye);
            }
            State = MasterState.Left;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
            base.Dispose(disposing);
        }
    }
}
