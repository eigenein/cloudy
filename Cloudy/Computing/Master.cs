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
            get { return state; }
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

        public event ParametrizedEventHandler<int> SlavesCleanedUp;

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
            slaves.Remove(slaveContext.ExternalEndPoint);
            if (SlaveLeft != null)
            {
                SlaveLeft(this, new EventArgs<SlaveContext>(slaveContext));
            }
        }

        private void StartNetwork(object state)
        {
            AssignAddresses(); // TODO: event
            AllocateThreads(); // TODO: event
            // RunThreads(); // TODO: event
            CleanUpSlaves(); // TODO: event
            this.state = MasterState.Running; // TODO: Add event to state change
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
                    mapping.Value.Threads.Add(context);
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
            throw new NotImplementedException();
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
        protected virtual void OnNetworkFailure()
        {
            // TODO: Shutdown all the slaves.
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
    }
}
