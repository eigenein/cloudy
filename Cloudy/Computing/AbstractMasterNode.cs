using System;
using System.Net;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Structures;
using Cloudy.Computing.Structures.Values;
using Cloudy.Computing.Topologies.Interfaces;
using Cloudy.Helpers;
using Cloudy.Messaging.Interfaces;

namespace Cloudy.Computing
{
    public abstract class AbstractMasterNode : AbstractNode
    {
        private MasterState state;

        private readonly ITopologyRepository topologyRepository;

        protected readonly INetworkRepository NetworkRepository;

        protected AbstractMasterNode(int port, INetworkRepository networkRepository,
            ITopologyRepository topologyRepository) 
            : base(port)
        {
            this.NetworkRepository = networkRepository;
            this.topologyRepository = topologyRepository;
            AddHandler(Tags.JoinRequest, OnJoinRequest);
            AddHandler(Tags.Bye, OnBye);
            AddHandler(Tags.ThreadCompleted, OnThreadCompleted);
            AddHandler(Tags.ThreadFailed, OnThreadFailed);
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

        public event ParametrizedEventHandler<Guid> ThreadCompleted;

        public event ParametrizedEventHandler<Guid> ThreadFailed;

        protected abstract ITopology Topology { get; }

        protected abstract void OnSlaveJoined(SlaveContext slave);

        protected abstract void OnSlaveLeft(SlaveContext slave);

        protected abstract void OnThreadFailedToStart(Guid slaveId, Guid threadId);

        protected abstract bool OnJobStopped(JobResult result);

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
            if (SlaveJoined != null)
            {
                SlaveJoined(this, new EventArgs<IPEndPoint, Guid>(remoteEndPoint, slaveId));
            }
            OnSlaveJoined(slave);
            return true;
        }

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

        private bool OnThreadFailed(IPEndPoint remoteEndPoint, IMessage message)
        {
            Guid threadId = message.Get<GuidValue>().Value;
            if (ThreadFailed != null)
            {
                ThreadFailed(this, new EventArgs<Guid>(threadId));
            }
            return true;
        }

        private bool OnThreadCompleted(IPEndPoint remoteEndPoint, IMessage message)
        {
            Guid threadId = message.Get<GuidValue>().Value;
            if (ThreadCompleted != null)
            {
                ThreadCompleted(this, new EventArgs<Guid>(threadId));
            }
            if (NetworkRepository.RemoveFromRunningThreadsCount(1) == 0)
            {
                StopJob(JobResult.Succeeded);
            }
            return true;
        }

        protected void CreateThreads(SlaveContext slave)
        {
            int count = slave.SlotsCount - NetworkRepository.GetThreadsCount(slave.SlaveId);
            while (count-- > 0)
            {
                ThreadContext thread = new ThreadContext();
                thread.ThreadId = Guid.NewGuid();
                thread.State = Topology.TryAddThread(thread.ThreadId, topologyRepository) ?
                    ThreadState.NotRunning : ThreadState.Reserved;
                NetworkRepository.AddThread(slave.SlaveId, thread);
            }
        }

        protected bool Start()
        {
            bool atLeastOneStarted = false;
            NetworkRepository.ResetRunningThreadsCount();
            foreach (SlaveContext slave in NetworkRepository.GetAllSlaves())
            {
                foreach (ThreadContext thread in NetworkRepository.GetThreads(slave.SlaveId))
                {
                    if (thread.State == ThreadState.Reserved)
                    {
                        continue;
                    }
                    if (StartingThread != null)
                    {
                        StartingThread(this, new EventArgs<Guid>(thread.ThreadId));
                    }
                    try
                    {
                        Send(slave.ExternalEndPoint, new GuidValue { Value = thread.ThreadId }, 
                            Tags.StartThread);
                        NetworkRepository.SetThreadState(thread.ThreadId, ThreadState.Running);
                        NetworkRepository.AddToRunningThreadsCount(1);
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

        protected void StopJob(JobResult result)
        {
            StopAllThreads();
            if (JobStopped != null)
            {
                JobStopped(this, new EventArgs<JobResult>(result));
            }
            if (OnJobStopped(result))
            {
                State = MasterState.Joined;
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
                            NetworkRepository.SetThreadState(thread.ThreadId,
                                ThreadState.NotRunning);
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
