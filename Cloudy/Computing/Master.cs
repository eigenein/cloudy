using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Messaging.Structures;
using Cloudy.Computing.Structures;
using Cloudy.Computing.Topologies;
using Cloudy.Computing.Topologies.Structures;
using Cloudy.Helpers;
using Cloudy.Messaging.Enums;
using Cloudy.Messaging.Interfaces;
using Cloudy.Messaging.Structures;

namespace Cloudy.Computing
{
    /// <summary>
    /// Represents an abstract master node.
    /// </summary>
    public abstract class Master : Node
    {
        private readonly Dictionary<IPEndPoint, SlaveContext> slaves =
            new Dictionary<IPEndPoint, SlaveContext>();

        private readonly Dictionary<ThreadAddress, ThreadContext> threads =
            new Dictionary<ThreadAddress, ThreadContext>();

        private readonly Topology topology;

        protected MasterState state = MasterState.AwaitingForSlaves;

        private int totalThreadSlotsCount;

        // private int runningThreadsCount;

        protected Master(int port, Topology topology)
            : base(port)
        {
            this.topology = topology;
        }

        /// <summary>
        /// Gets the count of available threads to start threads at the first time.
        /// </summary>
        public abstract int StartThreadsCount { get; }

        public MasterState State
        {
            get
            { 
                return state; 
            }
            private set
            {
                if (state == value)
                {
                    return;
                }
                state = value;
                if (StateChanged != null)
                {
                    StateChanged(this, new EventArgs<MasterState>(value));
                }
            }
        }

        /// <summary>
        /// Gets the total count of thread slots within all the joined slaves.
        /// </summary>
        public int TotalThreadSlotsCount
        {
            get { return totalThreadSlotsCount; }
        }

        public int RunningThreadsCount
        {
            get { throw new NotImplementedException(); }
        }

        public event ParametrizedEventHandler<SlaveContext> SlaveJoined;

        public event ParametrizedEventHandler<SlaveContext> SlaveLeft;

        public event ParametrizedEventHandler<int> ThreadsRun;

        public event ParametrizedEventHandler<int> ThreadsAllocated;

        public event EventHandler InterconnectionsSettingUp;

        public event ParametrizedEventHandler<int> SlavesCleanedUp;

        public event ParametrizedEventHandler<MasterState> StateChanged;

        public override int ProcessIncomingMessages(int count)
        {
            int processedMessagesCount = Dispatcher.ProcessIncomingMessages(count);
            while (state != MasterState.Left && Dispatcher.Available > 0)
            {
                int? tag;
                IPEndPoint remoteEndPoint;
                IValue message = Dispatcher.Receive(out remoteEndPoint, out tag);
                switch (tag)
                {
                    case CommonTags.JoinRequest:
                        JoinRequestValue joinRequestValue = message.Get<JoinRequestValue>();
                        SlaveContext slaveContext = new SlaveContext()
                        {
                            LocalEndPoint = joinRequestValue.LocalEndPoint,
                            ExternalEndPoint = remoteEndPoint,
                            Metadata = joinRequestValue.Metadata,
                            SlotsCount = joinRequestValue.SlotsCount,
                            State = SlaveState.Joined
                        };
                        Dispatcher.BeginSend(remoteEndPoint, 
                            new JoinResponseValue(remoteEndPoint, topology.TopologyType),
                            CommonTags.JoinResponse, JoinResponseAsyncCallback, slaveContext);
                        break;
                    case CommonTags.Bye:
                        OnSlaveLeft(slaves[remoteEndPoint]);
                        break;
                }
                processedMessagesCount += 1;
            }
            return processedMessagesCount;
        }

        protected virtual void OnSlaveJoined(SlaveContext slaveContext)
        {
            totalThreadSlotsCount += slaveContext.SlotsCount;
            ParametrizedEventHandler<SlaveContext> handler = SlaveJoined;
            if (handler != null)
            {
                handler(this, new EventArgs<SlaveContext>(slaveContext));
            }
            if (state == MasterState.AwaitingForSlaves &&
                totalThreadSlotsCount >= StartThreadsCount)
            {
                ThreadPool.QueueUserWorkItem(StartNetwork);
            }
        }

        protected virtual void OnSlaveLeft(SlaveContext slaveContext)
        {
            totalThreadSlotsCount -= slaveContext.SlotsCount;
            foreach (ThreadContext thread in slaveContext.Threads)
            {
                threads.Remove(thread.Address);
            }
            slaves.Remove(slaveContext.ExternalEndPoint);
            if (SlaveLeft != null)
            {
                SlaveLeft(this, new EventArgs<SlaveContext>(slaveContext));
            }
        }

        private void StartNetwork(object state)
        {
            AllocateThreads();
            SetUpInterconnections();
            RunThreads();
            CleanUpSlaves();
            this.State = MasterState.Running;
        }

        protected int AllocateThreads()
        {
            int count = 0;
            topology.Allocate(totalThreadSlotsCount);
            IEnumerator<ThreadAddress> addressEnumerator = topology.GetEnumerator();
            bool outOfAddresses = false;
            foreach (KeyValuePair<IPEndPoint, SlaveContext> mapping in slaves)
            {
                if (outOfAddresses)
                {
                    break;
                }
                if (mapping.Value.State != SlaveState.Joined)
                {
                    continue;
                }
                for (int i = 0; i < mapping.Value.SlotsCount - mapping.Value.Threads.Count; i++)
                {
                    if (!addressEnumerator.MoveNext())
                    {
                        outOfAddresses = true;
                        break;
                    }
                    ThreadAddress address = addressEnumerator.Current;
                    try
                    {
                        Dispatcher.Send(mapping.Key, address,
                            CommonTags.AllocateThread, ReceiptTimeout);
                    }
                    catch (TimeoutException)
                    {
                        mapping.Value.State = SlaveState.Left;
                        break;
                    }
                    ThreadContext thread = new ThreadContext();
                    thread.Address = address;
                    thread.State = Enums.ThreadState.NotRunning;
                    thread.SlaveContext = mapping.Value;
                    threads[address] = thread;
                    mapping.Value.Threads.Add(thread);
                    count += 1;
                }
            }
            if (ThreadsAllocated != null)
            {
                ThreadsAllocated(this, new EventArgs<int>(count));
            }
            return count;
        }

        protected int SetUpInterconnections(IPEndPoint endPoint, SlaveContext slave)
        {
            int count = 0;
            foreach (ThreadContext thread in slave.Threads)
            {
                if (thread.State != Enums.ThreadState.NotRunning)
                {
                    continue;
                }
                try
                {
                    foreach (ThreadAddress neighborAddress in
                        topology.GetNeighbors(thread.Address))
                    {
                        SlaveContext neighborContext = threads[neighborAddress].SlaveContext;
                        InterconnectionValue value = new InterconnectionValue();
                        // The current thread address will be local for it.
                        value.LocalThreadAddress = thread.Address;
                        // While the neighbor address will be remote for it.
                        value.RemoteThreadAddress = neighborAddress;
                        // Endpoints of the neighbor.
                        value.LocalEndPoint = neighborContext.LocalEndPoint;
                        value.ExternalEndPoint = neighborContext.ExternalEndPoint;
                        Dispatcher.Send(endPoint, value, CommonTags.Interconnection, 
                            ResponseTimeout);
                    }
                }
                catch (TimeoutException)
                {
                    slave.State = SlaveState.Left;
                    break;
                }
                count += 1;
            }
            return count;
        }

        protected void SetUpInterconnections()
        {
            if (InterconnectionsSettingUp != null)
            {
                InterconnectionsSettingUp(this, new EventArgs());
            }
            foreach (KeyValuePair<IPEndPoint, SlaveContext> mapping in slaves)
            {
                if (mapping.Value.State != SlaveState.Joined)
                {
                    continue;
                }
                SetUpInterconnections(mapping.Key, mapping.Value);
            }
        }

        /// <summary>
        /// Cleans up the left slaves.
        /// </summary>
        /// <returns>Slaves cleaned up count.</returns>
        protected int CleanUpSlaves()
        {
            int count = 0;
            foreach (SlaveContext slave in slaves.Values)
            {
                if (slave.State == SlaveState.Left)
                {
                    OnSlaveLeft(slave);
                    count += 1;
                }
            }
            if (SlavesCleanedUp != null)
            {
                SlavesCleanedUp(this, new EventArgs<int>(count));
            }
            return count;
        }

        /// <summary>
        /// Starts computational threads.
        /// </summary>
        /// <returns>Running threads count.</returns>
        protected int RunThreads()
        {
            int count = 0;
            if (ThreadsRun != null)
            {
                ThreadsRun(this, new EventArgs<int>(count));
            }
            return count;
        }

        /// <summary>
        /// Called when the network is finally failed.
        /// </summary>
        protected virtual void OnJobCompleted(bool success, string message)
        {
            ShutdownSlaves();
            State = MasterState.Left;
        }

        /// <summary>
        /// Called when a slave receives a join response.
        /// </summary>
        private void JoinResponseAsyncCallback(IAsyncResult ar)
        {
            // Now we know for sure that the slave thinks that is was joined.
            // And we can add it to our lists.
            SlaveContext slaveContext = (SlaveContext)ar.AsyncState;
            slaves[slaveContext.ExternalEndPoint] = slaveContext;
            OnSlaveJoined(slaveContext);
        }

        public void ShutdownSlaves()
        {
            foreach (KeyValuePair<IPEndPoint, SlaveContext> mapping in slaves)
            {
                Dispatcher.Send(mapping.Key, EmptyValue.Instance, CommonTags.Bye);
            }
        }

        protected override void Dispose(bool dispose)
        {
            State = MasterState.Left;
            base.Dispose(dispose);
        }
    }
}
