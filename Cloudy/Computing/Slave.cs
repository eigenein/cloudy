using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Messaging.Structures;
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

        public event ParametrizedEventHandler<Tuple<ThreadAddress, IPEndPoint>>
            InterconnectionEstablished;

        public event ParametrizedEventHandler<IPEndPoint> ConnectingFailed;

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
                        // To prevent pausing of messages handling.
                        ThreadPool.QueueUserWorkItem(o =>
                            EstablishInterconnection(message.Get<NeighborValue>()));
                        break;
                }
            }
            return processedMessagesCount;
        }

        private void OnLeft(IPEndPoint recipientEndPoint)
        {
            Dispatcher.Send(recipientEndPoint, EmptyValue.Instance, CommonTags.Bye);
            State = SlaveState.Left;
        }

        private bool EstablishInterconnection(IPEndPoint endPoint,
            out TimeSpan timeElapsed)
        {
            bool success;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                Dispatcher.Ping(endPoint, ReceiptTimeout);
                success = true;
            }
            catch (TimeoutException)
            {
                stopwatch.Reset();
                stopwatch.Start();
                try
                {
                    Dispatcher.Ping(endPoint, ReceiptTimeout);
                    success = true;
                }
                catch (TimeoutException)
                {
                    success = false;
                }
            }
            finally
            {
                stopwatch.Stop();
                timeElapsed = stopwatch.Elapsed;
            }
            if (!success && ConnectingFailed != null)
            {
                ConnectingFailed(this, new EventArgs<IPEndPoint>(endPoint));
            }
            return success;
        }

        private bool EstablishInterconnection(NeighborValue neighbor)
        {
            if (InterconnectionEstablishing != null)
            {
                InterconnectionEstablishing(this, new EventArgs<NeighborValue>(neighbor));
            }
            TimeSpan localTimeElapsed, externalTimeElapsed;
            bool localSucceeded = EstablishInterconnection(neighbor.LocalEndPoint,
                out localTimeElapsed);
            bool externalSucceeded = EstablishInterconnection(neighbor.ExternalEndPoint,
                out externalTimeElapsed);
            if (!localSucceeded && !externalSucceeded)
            {
                return false;
            }
            if (localSucceeded && !externalSucceeded)
            {
                OnInterconnectionEstablished(neighbor.ThreadAddress, neighbor.LocalEndPoint);
                return true;
            }
            if (!localSucceeded)
            {
                OnInterconnectionEstablished(neighbor.ThreadAddress, neighbor.ExternalEndPoint);
                return true;
            }
            OnInterconnectionEstablished(neighbor.ThreadAddress,
                localTimeElapsed < externalTimeElapsed ? 
                neighbor.LocalEndPoint : neighbor.ExternalEndPoint);
            return true;
        }

        private void OnInterconnectionEstablished(ThreadAddress threadAddress,
            IPEndPoint endPoint)
        {
            neighbors[threadAddress] = endPoint;
            if (InterconnectionEstablished != null)
            {
                InterconnectionEstablished(this, new EventArgs<Tuple<ThreadAddress, IPEndPoint>>(
                    new Tuple<ThreadAddress, IPEndPoint>(threadAddress, endPoint)));
            }
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
