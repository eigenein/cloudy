using System;
using System.Collections.Generic;
using System.Net;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Messaging.Structures;
using Cloudy.Computing.Structures;
using Cloudy.Computing.Topologies.Structures;
using Cloudy.Helpers;
using Cloudy.Messaging.Enums;
using Cloudy.Messaging.Interfaces;
using Cloudy.Messaging.Structures;
using Cloudy.Structures;

namespace Cloudy.Computing
{
    /// <summary>
    /// Represents an abstract slave node.
    /// </summary>
    public abstract class Slave : Node
    {
        private readonly IPEndPoint localEndPoint;

        private readonly Dictionary<ThreadAddress, IPEndPoint> neighbors =
            new Dictionary<ThreadAddress, IPEndPoint>();

        private IPEndPoint masterEndPoint;

        private SlaveState state = SlaveState.NotJoined;

        protected Slave(IPEndPoint localEndPoint) 
            : base(localEndPoint.Port)
        {
            this.localEndPoint = localEndPoint;
        }

        public abstract int SlotsCount { get; }

        public SlaveState State
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
                    StateChanged(this, new EventArgs<SlaveState>(value));
                }
            }
        }

        /// <summary>
        /// Occurs when a slaves successfully joins the network.
        /// </summary>
        public event ParametrizedEventHandler<IPEndPoint> Joined;

        /// <summary>
        /// Occurs when a thread is allocated in the slave.
        /// </summary>
        public event ParametrizedEventHandler<AllocateThreadValue> ThreadAllocated;

        public event ParametrizedEventHandler<SlaveState> StateChanged;

        public event ParametrizedEventHandler<NeighborValue> InterconnectionEstablishing;

        /// <summary>
        /// Runs a computation.
        /// </summary>
        protected abstract void Run(IThreadWorld world);

        /// <summary>
        /// Joins a network.
        /// </summary>
        /// <param name="remoteEndPoint">The master endpoint.</param>
        /// <param name="metadata">The metadata that will be associated with the slave.</param>
        public void JoinNetwork(IPEndPoint remoteEndPoint, byte[] metadata)
        {
            if (state != SlaveState.NotJoined)
            {
                throw new InvalidOperationException("Can join in NotJoined state only.");
            }
            Dispatcher.Send(remoteEndPoint, 
                new JoinRequestValue(localEndPoint, SlotsCount, metadata), 
                CommonTags.JoinRequest, ReceiptTimeout);
            JoinResponseValue response = Dispatcher.Receive<JoinResponseValue>(
                ResponseTimeout);
            State = SlaveState.Joined;
            if (Joined != null)
            {
                masterEndPoint = remoteEndPoint;
                Joined(this, new EventArgs<IPEndPoint>(response.ExternalEndPoint));
            }
        }

        public override int ProcessIncomingMessages(int count)
        {
            int processedMessagesCount = Dispatcher.ProcessIncomingMessages(count);
            while (state == SlaveState.Joined && Dispatcher.Available > 0)
            {
                int? tag;
                IPEndPoint remoteEndPoint;
                IValue message = Dispatcher.Receive(out remoteEndPoint, out tag);
                switch (tag)
                {
                    case CommonTags.AllocateThread:
                        AllocateThreadValue value = message.Get<AllocateThreadValue>();
                        // TODO: Actually create a thread.
                        if (ThreadAllocated != null)
                        {
                            ThreadAllocated(this, new EventArgs<AllocateThreadValue>(value));
                        }
                        break;
                    case CommonTags.Bye:
                        OnLeft(remoteEndPoint);
                        break;
                    case CommonTags.Neighbor:
                        EstablishInterconnection(message.Get<NeighborValue>());
                        // TODO: Send the result to the master.
                        break;
                }
            }
            return processedMessagesCount;
        }

        private void OnLeft(IPEndPoint recipientEndPoint)
        {
            Dispatcher.Send(recipientEndPoint, new ByeValue(), CommonTags.Bye);
            State = SlaveState.Left;
        }

        private bool EstablishInterconnection(NeighborValue neighbor)
        {
            if (InterconnectionEstablishing != null)
            {
                InterconnectionEstablishing(this, new EventArgs<NeighborValue>(neighbor));
            }
            // TODO
            return false;
        }

        protected override void Dispose(bool dispose)
        {
            if (state == SlaveState.Joined)
            {
                OnLeft(masterEndPoint);
            }
            base.Dispose(dispose);
        }
    }
}
