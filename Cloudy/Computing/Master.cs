using System;
using System.Collections.Generic;
using System.Net;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Events;
using Cloudy.Computing.Exceptions;
using Cloudy.Computing.Messaging.Structures;
using Cloudy.Computing.Structures;
using Cloudy.Computing.Topologies;
using Cloudy.Messaging.Enums;
using Cloudy.Messaging.Interfaces;

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

        private int runningThreadsCount;

        protected Master(int port, Topology topology)
            : base(port)
        {
            this.topology = topology;
        }

        /// <summary>
        /// Gets the minimum joined threads count for the network.
        /// </summary>
        public abstract int MinimumThreadsCount { get; }

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
            get { return runningThreadsCount; }
        }

        public event EventHandler<SlaveJoinedEventArgs> SlaveJoined;

        public event EventHandler<SlaveLeftEventArgs> SlaveLeft;

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
                            SlotsCount = joinRequestValue.SlotsCount
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
            EventHandler<SlaveJoinedEventArgs> handler = SlaveJoined;
            if (handler != null)
            {
                handler(this, new SlaveJoinedEventArgs(slaveContext));
            }
            if (state == MasterState.AwaitingForSlaves &&
                totalThreadSlotsCount >= MinimumThreadsCount)
            {
                // TODO: allocate and run.
                state = MasterState.Running;
            }
        }

        protected virtual void OnSlaveLeft(SlaveContext slaveContext)
        {
            totalThreadSlotsCount -= slaveContext.SlotsCount;
            slaves.Remove(slaveContext.ExternalEndPoint);
            foreach (ThreadContext thread in slaveContext.Threads)
            {
                OnNeighborLeft(thread.Address);
            }
            if (SlaveLeft != null)
            {
                SlaveLeft(this, new SlaveLeftEventArgs(slaveContext));
            }
            if (totalThreadSlotsCount < MinimumThreadsCount)
            {
                OnNetworkFailure();
                // TODO: Perform pausing (optionally) until slots are enough.
                throw new NetworkFailure("There are not enough thread slots.");
            }
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
