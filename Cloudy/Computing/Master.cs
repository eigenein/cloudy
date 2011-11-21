using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Exceptions;
using Cloudy.Computing.Messaging.Structures;
using Cloudy.Computing.Structures;
using Cloudy.Computing.Topologies;
using Cloudy.Computing.Topologies.Structures;
using Cloudy.Helpers;
using Cloudy.Messaging.Enums;
using Cloudy.Messaging.Interfaces;
using Cloudy.Messaging.Structures;
using ThreadState = Cloudy.Computing.Enums.ThreadState;

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

        public event ParametrizedEventHandler<int> AddressesAssigned;

        public event ParametrizedEventHandler<int> ThreadsAllocated;

        public event ParametrizedEventHandler<int> InterconnectionsSetup;

        public event ParametrizedEventHandler<int> SlavesCleanedUp;

        public event ParametrizedEventHandler<MasterState> StateChanged;

        public override int ProcessIncomingMessages(int count)
        {
            int processedMessagesCount = Dispatcher.ProcessIncomingMessages(count);
            while (state != MasterState.Left && Dispatcher.Available > 0)
            {
                int? tag;
                IPEndPoint remoteEndPoint;
                ICastable message = Dispatcher.Receive(out remoteEndPoint, out tag);
                switch (tag)
                {
                    case CommonTags.JoinRequest:
                        JoinRequestValue joinRequestValue = message.Cast<JoinRequestValue>();
                        SlaveContext slaveContext = new SlaveContext()
                        {
                            LocalEndPoint = joinRequestValue.LocalEndPoint,
                            ExternalEndPoint = remoteEndPoint,
                            Metadata = joinRequestValue.Metadata,
                            SlotsCount = joinRequestValue.SlotsCount,
                            State = SlaveState.Joined
                        };
                        Dispatcher.BeginSend(remoteEndPoint, new JoinResponseValue(remoteEndPoint),
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
            AssignAddresses();
            AllocateThreads();
            SetupInterconnections(); // TODO: event
            // RunThreads(); // TODO: event
            CleanUpSlaves();
            this.State = MasterState.Running;
        }

        /// <summary>
        /// Assigns addresses to the available threads.
        /// </summary>
        /// <returns>Assigned addresses count.</returns>
        protected int AssignAddresses()
        {
            int count = 0;
            topology.Allocate(totalThreadSlotsCount);
            IEnumerator<ThreadAddress> addressEnumerator = topology.GetEnumerator();
            foreach (KeyValuePair<IPEndPoint, SlaveContext> mapping in slaves)
            {
                for (int i = 0; i < mapping.Value.SlotsCount; i++)
                {
                    if (!addressEnumerator.MoveNext())
                    {
                        break;
                    }
                    ThreadContext context = new ThreadContext();
                    context.Address = addressEnumerator.Current;
                    context.State = ThreadState.Initial;
                    context.SlaveContext = mapping.Value;
                    mapping.Value.Threads.Add(context);
                    threads.Add(addressEnumerator.Current, context);
                    count += 1;
                }
            }
            if (AddressesAssigned != null)
            {
                AddressesAssigned(this, new EventArgs<int>(count));
            }
            return count;
        }

        /// <summary>
        /// Sends allocation messages to slaves.
        /// </summary>
        /// <returns>Successfully allocated threads count.</returns>
        protected int AllocateThreads()
        {
            int count = 0;
            foreach (KeyValuePair<IPEndPoint, SlaveContext> mapping in slaves)
            {
                foreach (ThreadContext thread in mapping.Value.Threads)
                {
                    if (mapping.Value.State != SlaveState.Joined)
                    {
                        break;
                    }
                    if (thread.State != ThreadState.Initial)
                    {
                        continue;
                    }
                    AllocateThreadValue value = new AllocateThreadValue();
                    value.ThreadAddress = thread.Address;
                    value.TopologyType = topology.TopologyType;
                    MessagingAsyncResult ar = Dispatcher.BeginSend(
                        mapping.Key, value, CommonTags.AllocateThread,
                        null, null);
                    try
                    {
                        Dispatcher.EndSend(ar, ResponseTimeout);
                        thread.State = ThreadState.Allocated;
                        count += 1;
                    }
                    catch (TimeoutException)
                    {
                        mapping.Value.State = SlaveState.Left;
                    }
                }
            }
            if (ThreadsAllocated != null)
            {
                ThreadsAllocated(this, new EventArgs<int>(count));
            }
            return count;
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
        /// Initializes interconnections between slaves.
        /// </summary>
        /// <returns>Connections initalized count.</returns>
        protected int SetupInterconnections()
        {
            int count = 0;
            foreach (KeyValuePair<IPEndPoint, SlaveContext> mapping in slaves)
            {
                if (mapping.Value.State != SlaveState.Joined)
                {
                    continue;
                }
                HashSet<ThreadAddress> addresses = new HashSet<ThreadAddress>();
                foreach (ThreadContext thread in mapping.Value.Threads)
                {
                    if (thread.State == ThreadState.Allocated)
                    {
                        addresses.UnionWith(topology.GetNeighbors(thread.Address));
                    }
                }
                foreach (ThreadAddress neighbor in addresses)
                {
                    Dispatcher.BeginSend(mapping.Key,
                        new NeighborValue(neighbor, threads[neighbor].SlaveContext),
                        CommonTags.Neighbor, null, null);
                }
            }
            if (InterconnectionsSetup != null)
            {
                InterconnectionsSetup(this, new EventArgs<int>(count));
            }
            return count;
        }

        /// <summary>
        /// Starts computational threads.
        /// </summary>
        /// <returns>Running threads count.</returns>
        protected bool RunThreads()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called when the network is finally failed.
        /// </summary>
        protected virtual void OnNetworkFailure(string message)
        {
            ShutdownSlaves();
            throw new NetworkFailure(message);
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
                Dispatcher.BeginSend(mapping.Key, new ByeValue(), CommonTags.Bye, null, null);
            }
        }
    }
}
