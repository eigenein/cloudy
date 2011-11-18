using System;
using System.Collections.Generic;
using System.Net;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Events;
using Cloudy.Computing.Messaging.Structures;
using Cloudy.Computing.Structures;
using Cloudy.Computing.Topologies.Enums;
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

        protected MasterState state = MasterState.Joined;

        protected Master(int port)
            : base(port)
        {
        }

        /// <summary>
        /// Gets topologies that are used in the network.
        /// </summary>
        public abstract TopologyType[] UsedTopologies { get; }

        /// <summary>
        /// Gets the minimum joined threads count for the network.
        /// </summary>
        public abstract int MinimumThreadsCount { get; }

        public MasterState State
        {
            get { return state; }
        }

        public override int ProcessIncomingMessages(int count)
        {
            int processedMessagesCount = Dispatcher.ProcessIncomingMessages(count);
            while (state == MasterState.Joined && Dispatcher.Available > 0)
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
                            Metadata = joinRequestValue.Metadata
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
            EventHandler<SlaveJoinedEventArgs> handler = SlaveJoined;
            if (handler != null)
            {
                handler(this, new SlaveJoinedEventArgs(slaveContext));
            }
        }

        protected virtual void OnSlaveLeft(SlaveContext slaveContext)
        {
            EventHandler<SlaveLeftEventArgs> handler = SlaveLeft;
            if (handler != null)
            {
                handler(this, new SlaveLeftEventArgs(slaveContext));
            }
        }

        public event EventHandler<SlaveJoinedEventArgs> SlaveJoined;

        public event EventHandler<SlaveLeftEventArgs> SlaveLeft;

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
