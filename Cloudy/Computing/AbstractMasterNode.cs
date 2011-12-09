using System;
using System.Collections.Generic;
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
            AddHandler(Tags.RouteRequest, OnRouteRequest);
            AddHandler(Tags.ResolveRecipientRequest, OnResolveRecipient);
            MessageHandled += OnMessageHandled;
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

        /// <summary>
        /// Notifies that a slave has left.
        /// </summary>
        /// <returns>
        /// Whether a master should attempt to continue the job (failed otherwise).
        /// </returns>
        protected abstract bool OnSlaveLeft(SlaveContext slave);

        /// <summary>
        /// Notifies that the thread could not be started.
        /// </summary>
        /// <returns>
        /// Whether starting of a job should be continued (failed otherwise).
        /// </returns>
        protected abstract bool OnThreadFailedToStart(Guid slaveId, Guid threadId);

        /// <summary>
        /// Notifies that the job is stopped.
        /// </summary>
        /// <returns>
        /// Whether a master should continue with a new job (closing otherwise).
        /// </returns>
        protected abstract bool OnJobStopped(JobResult result);

        /// <summary>
        /// Called when the master receives a join request from a slave.
        /// </summary>
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
            if (SlaveLeft != null)
            {
                SlaveLeft(this, new EventArgs<IPEndPoint, Guid>(remoteEndPoint, slave.SlaveId));
            }
            if (!OnSlaveLeft(slave))
            {
                StopJob(JobResult.Failed);
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

        private void StopThread(IPEndPoint remoteEndPoint, Guid threadId)
        {
            SendAsync(remoteEndPoint, new GuidValue { Value = threadId },
                Tags.StopThread);
        }

        private bool OnRouteRequest(IPEndPoint remoteEndPoint, IMessage message)
        {
            RouteRequestValue value = message.Get<RouteRequestValue>();
            Guid nextThreadId;
            if (!Topology.TryGetRoute(value.CurrentThreadId, value.DestinationThreadId,
                topologyRepository, out nextThreadId))
            {
                // We don't know this destination, thus terminate the thread.
                StopThread(remoteEndPoint, value.CurrentThreadId);
                return true;
            }
            // Ok, send the next thread ID back.
            SendAsync(remoteEndPoint, new GuidValue { Value = nextThreadId },
                Tags.RouteResponse);
            return true;
        }

        private bool OnResolveRecipient(IPEndPoint remoteEndPoint, IMessage message)
        {
            ResolveRecipientRequestValue request =
                message.Get<ResolveRecipientRequestValue>();
            ResolveRecipientResponseValue response = new ResolveRecipientResponseValue();
            if (request.RecipientId == Guid.Empty)
            {
                // Resolve the loopback ID to the thread itself.
                response.ResolvedTo = new[] { request.CurrentThreadId };
            }
            if (!Topology.IsShortcut(request.RecipientId))
            {
                response.ResolvedTo = new[] { request.RecipientId };
            }
            ICollection<Guid> resolvedTo;
            if (topologyRepository.TryGetThreadsByShortcut(request.CurrentThreadId, 
                request.RecipientId, out resolvedTo))
            {
                // HashSet couldn't be cast to ICollection in runtime.
                response.ResolvedTo = new List<Guid>(resolvedTo);
                SendAsync(remoteEndPoint, response, Tags.ResolveRecipientResponse);
            }
            else
            {
                StopThread(remoteEndPoint, request.CurrentThreadId);
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
            if (state == MasterState.Running &&
                NetworkRepository.RemoveFromRunningThreadsCount(1) == 0)
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

        private void OnMessageHandled(object sender, EventArgs e)
        {
            if (state == MasterState.RestartPending)
            {
                if (!Start())
                {
                    State = MasterState.Joined;
                }
            }
        }

        protected virtual bool Start()
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
                        if (!OnThreadFailedToStart(slave.SlaveId, thread.ThreadId))
                        {
                            StopAllThreads();
                            OnFailedToStart();
                            return false;
                        }
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
            else
            {
                OnFailedToStart();
            }
            return atLeastOneStarted;
        }

        private void OnFailedToStart()
        {
            if (FailedToStart != null)
            {
                FailedToStart(this, new EventArgs());
            }
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
                State = MasterState.RestartPending;
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

        /// <summary>
        /// Silently shuts down the whole network.
        /// </summary>
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
