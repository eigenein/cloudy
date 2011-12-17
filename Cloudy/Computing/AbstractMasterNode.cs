using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Structures;
using Cloudy.Computing.Structures.Values;
using Cloudy.Computing.Topologies.Interfaces.Master;
using Cloudy.Helpers;
using Cloudy.Messaging.Interfaces;
using Cloudy.Structures;

namespace Cloudy.Computing
{
    public abstract class AbstractMasterNode : AbstractNode
    {
        private readonly Dictionary<Guid, SlaveContext> slaveById =
            new Dictionary<Guid, SlaveContext>();

        private readonly Dictionary<IPEndPoint, SlaveContext> slaveByEndPoint =
            new Dictionary<IPEndPoint, SlaveContext>();

        private readonly Dictionary<byte[], SlaveContext> slaveByRank =
            new Dictionary<byte[], SlaveContext>(ByteArrayComparer.Instance);

        protected int TotalSlotsCount;

        private int runningThreadsCount;

        private MasterState state;

        protected AbstractMasterNode(int port) 
            : base(port)
        {
            AddHandler(Tags.JoinRequest, HandleJoinRequest);
            AddHandler(Tags.Bye, HandleBye);
            AddHandler(Tags.ThreadCompleted, HandleThreadCompleted);
            AddHandler(Tags.ThreadFailed, HandleThreadFailed);
            AddHandler(Tags.EndPointRequest, HandleEndPointRequest);
            AddHandler(Tags.SignedPingRequest, HandleSignedPingRequest);
            AddHandler(Tags.SignedPingResponse, HandleSignedPingResponse);
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

        public event ParameterizedEventHandler<IPEndPoint, Guid> SlaveJoined;

        public event ParameterizedEventHandler<MasterState> StateChanged;

        public event ParameterizedEventHandler<IPEndPoint, Guid> SlaveLeft;

        public event ParameterizedEventHandler<JobResult> JobStopped;

        public event ParameterizedEventHandler<Guid, byte[]> ThreadFailedToStart;

        public event EventHandler Started;

        public event EventHandler JobFailedToStart;

        public event ParameterizedEventHandler<byte[]> StartingThread;

        public event ParameterizedEventHandler<byte[]> ThreadCompleted;

        public event ParameterizedEventHandler<byte[]> ThreadFailed;

        public event ParameterizedEventHandler<byte[], byte[]> RankReassigned;

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
        protected abstract bool OnThreadFailedToStart(Guid slaveId, byte[] threadRank);

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
        private void HandleJoinRequest(IPEndPoint remoteEndPoint, IMessage message)
        {
            JoinRequestValue request = message.Get<JoinRequestValue>();
            Guid slaveId = request.SlaveId ?? Guid.NewGuid();
            JoinResponseValue response = new JoinResponseValue();
            response.ExternalEndPoint = new EndPointValue { Value = remoteEndPoint };
            response.SlaveId = slaveId;
            response.TopologyType = Topology.TopologyType;
            try
            {
                Send(remoteEndPoint, response, Tags.JoinResponse);
            }
            catch (TimeoutException)
            {
                SendAsync(remoteEndPoint, EmptyValue.Instance, Tags.Bye);
                return;
            }
            SlaveContext slave;
            if (!slaveById.TryGetValue(slaveId, out slave))
            {
                Interlocked.Add(ref TotalSlotsCount, request.SlotsCount);
                slave = new SlaveContext();
                slave.SlaveId = slaveId;
                slave.SlotsCount = request.SlotsCount;
                slave.LocalEndPoint = request.LocalEndPoint.Value;
                slave.ExternalEndPoint = remoteEndPoint;
                slaveByEndPoint.Add(remoteEndPoint, slave);
                slaveById.Add(slaveId, slave);
            }
            if (SlaveJoined != null)
            {
                SlaveJoined(this, new EventArgs<IPEndPoint, Guid>(remoteEndPoint, slaveId));
            }
            OnSlaveJoined(slave);
        }

        private void HandleBye(IPEndPoint remoteEndPoint, IMessage message)
        {
            SlaveContext slave = slaveByEndPoint[remoteEndPoint];
            RemoveSlave(slave);
            if (SlaveLeft != null)
            {
                SlaveLeft(this, new EventArgs<IPEndPoint, Guid>(remoteEndPoint, slave.SlaveId));
            }
            if (!OnSlaveLeft(slave))
            {
                StopJob(JobResult.Failed);
            }
        }

        /// <summary>
        /// Removes the slave and its threads from all the caches.
        /// </summary>
        private void RemoveSlave(SlaveContext slave)
        {
            Interlocked.Add(ref TotalSlotsCount, -slave.SlotsCount);
            slaveByEndPoint.Remove(slave.ExternalEndPoint);
            slaveById.Remove(slave.SlaveId);
            foreach (ThreadContext thread in slave.Threads)
            {
                RemoveThread(thread);
            }
        }

        private void RemoveThread(ThreadContext thread)
        {
            slaveByRank.Remove(thread.Rank);
            byte[] replacement;
            if (Topology.RemoveThread(thread.Rank, out replacement))
            {
                ThreadPool.QueueUserWorkItem(o => ReassignRank(thread, 
                    slaveByRank[replacement], replacement));
            }
        }

        /// <summary>
        /// Replaces the removed thread with the specified thread.
        /// </summary>
        private void ReassignRank(ThreadContext thread, SlaveContext replacementSlave,
            byte[] replacementRank)
        {
            try
            {
                ThreadContext replacementThread = replacementSlave.Threads.Find(
                    t => t.Rank.SameAs(replacementRank));
                ReassignRankValue value = new ReassignRankValue();
                value.OldRank = replacementRank;
                value.NewRank = thread.Rank;
                // It's important to send the message before updating the caches.
                Send(replacementSlave.ExternalEndPoint, value, Tags.ReassignRank);
                replacementThread.Rank = thread.Rank; // This was removed.
                slaveByRank[thread.Rank] = replacementSlave;
                slaveByRank.Remove(replacementRank); // This was existing, but now is re-assigned.
                if (RankReassigned != null)
                {
                    RankReassigned(this, new EventArgs<byte[], byte[]>(
                        replacementRank, thread.Rank));
                }
            }
            catch (TimeoutException)
            {
                RemoveSlave(replacementSlave);
            }
        }

        private void HandleThreadFailed(IPEndPoint remoteEndPoint, IMessage message)
        {
            byte[] threadRank = message.Get<WrappedValue<byte[]>>().Value;
            if (ThreadFailed != null)
            {
                ThreadFailed(this, new EventArgs<byte[]>(threadRank));
            }
        }

        private void HandleEndPointRequest(IPEndPoint remoteEndPoint, IMessage message)
        {
            byte[] targetThread = message.Get<WrappedValue<byte[]>>().Value;
            SlaveContext slave;
            EndPointResponseValue response = new EndPointResponseValue();
            if (slaveByRank.TryGetValue(targetThread, out slave))
            {
                response.ExternalEndPoint.Value = slave.ExternalEndPoint;
                response.LocalEndPoint.Value = slave.LocalEndPoint;
            }
            else
            {
                response.IsFound = false;
                /*
                 * Do not send these values (they will be skipped by
                 * Protocol Buffers serializer because of null values).
                 */
                response.LocalEndPoint = null;
                response.ExternalEndPoint = null;
            }
            ThreadPool.QueueUserWorkItem(o => Send(
                remoteEndPoint, response, Tags.EndPointResponse));
        }

        private void HandleSignedPingRequest(IPEndPoint remoteEndPoint, IMessage message)
        {
            SignedPingRequest request = message.Get<SignedPingRequest>();
            SlaveContext targetSlave;
            if (slaveByRank.TryGetValue(request.Destination, out targetSlave))
            {
                ThreadPool.QueueUserWorkItem(o => Send(
                    targetSlave.ExternalEndPoint, request, Tags.SignedPingRequest));
            }
        }

        private void HandleSignedPingResponse(IPEndPoint remoteEndPoint, IMessage message)
        {
            SignedPingResponse response = message.Get<SignedPingResponse>();
            IPEndPoint targetEndPoint = response.SenderExternalEndPoint.Value;
            response.SenderExternalEndPoint = null;
            SendAsync(targetEndPoint, response, Tags.SignedPingResponse);
        }

        private void HandleThreadCompleted(IPEndPoint remoteEndPoint, IMessage message)
        {
            byte[] threadRank = message.Get<WrappedValue<byte[]>>().Value;
            if (ThreadCompleted != null)
            {
                ThreadCompleted(this, new EventArgs<byte[]>(threadRank));
            }
            if (state == MasterState.Running &&
                Interlocked.Decrement(ref runningThreadsCount) == 0)
            {
                StopJob(JobResult.Succeeded);
            }
        }

        protected void CreateThreads(SlaveContext slave)
        {
            int count = slave.SlotsCount - slave.Threads.Count;
            while (count-- > 0)
            {
                ThreadContext thread = new ThreadContext();
                byte[] rank;
                thread.State = Topology.TryAddThread(out rank) ?
                    Enums.ThreadState.NotRunning : Enums.ThreadState.Reserved;
                thread.Rank = rank;
                slaveByRank.Add(rank, slave);
                slave.Threads.Add(thread);
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
            runningThreadsCount = 0;
            foreach (SlaveContext slave in slaveById.Values)
            {
                foreach (ThreadContext thread in slave.Threads)
                {
                    if (thread.State == Enums.ThreadState.Reserved)
                    {
                        continue;
                    }
                    if (StartingThread != null)
                    {
                        StartingThread(this, new EventArgs<byte[]>(thread.Rank));
                    }
                    try
                    {
                        WrappedValue<byte[]> value = new WrappedValue<byte[]>();
                        value.Value = thread.Rank;
                        Send(slave.ExternalEndPoint, value, Tags.StartThread);
                        thread.State = Enums.ThreadState.Running;
                        Interlocked.Increment(ref runningThreadsCount);
                        atLeastOneStarted = true;
                    }
                    catch (TimeoutException)
                    {
                        if (ThreadFailedToStart != null)
                        {
                            ThreadFailedToStart(this, new EventArgs<Guid, byte[]>(
                                slave.SlaveId, thread.Rank));
                        }
                        if (SlaveLeft != null)
                        {
                            SlaveLeft(this, new EventArgs<IPEndPoint, Guid>(
                                slave.ExternalEndPoint, slave.SlaveId));
                        }
                        RemoveSlave(slave);
                        // Check whether we should fail the entire network.
                        if (!OnThreadFailedToStart(slave.SlaveId, thread.Rank))
                        {
                            StopAllThreads();
                            OnJobFailedToStart();
                            return false;
                        }
                        // Break processing the current slave.
                        break;
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
                OnJobFailedToStart();
            }
            return atLeastOneStarted;
        }

        private void OnJobFailedToStart()
        {
            if (JobFailedToStart != null)
            {
                JobFailedToStart(this, new EventArgs());
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

        /// <summary>
        /// Stops all the threads in network.
        /// </summary>
        private void StopAllThreads()
        {
            foreach (SlaveContext slave in slaveById.Values)
            {
                foreach (ThreadContext thread in slave.Threads)
                {
                    if (thread.State == Enums.ThreadState.Running)
                    {
                        IPEndPoint targetEndPoint = slave.ExternalEndPoint;
                        ThreadContext targetThread = thread;
                        ThreadPool.QueueUserWorkItem(o => StopThread(
                            targetEndPoint, targetThread));
                    }
                }
            }
        }

        /// <summary>
        /// Stops the thread.
        /// </summary>
        /// <returns>Whether the thread was successfully stopped.</returns>
        private bool StopThread(IPEndPoint targetEndPoint, ThreadContext thread)
        {
            WrappedValue<byte[]> value = new WrappedValue<byte[]>();
            value.Value = thread.Rank;
            try
            {
                Send(targetEndPoint, value, Tags.StopThread);
                thread.State = Enums.ThreadState.NotRunning;
                return true;
            }
            catch (TimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// Silently shuts down the whole network.
        /// </summary>
        public void Close()
        {
            foreach (IPEndPoint endPoint in slaveByEndPoint.Keys)
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
