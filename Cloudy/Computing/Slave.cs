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
        public event ParametrizedEventHandler<ThreadAddress> ThreadAllocated;

        public event ParametrizedEventHandler<SlaveState> StateChanged;

        public event ParametrizedEventHandler<InterconnectionValue> InterconnectionEstablishing;

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
                        ThreadAddress value = message.Get<ThreadAddress>();
                        // TODO: Actually create a thread.
                        if (ThreadAllocated != null)
                        {
                            ThreadAllocated(this, new EventArgs<ThreadAddress>(value));
                        }
                        break;
                    case CommonTags.Bye:
                        OnLeft(remoteEndPoint);
                        break;
                    case CommonTags.Interconnection:
                        // To prevent pausing of messages handling.
                        ThreadPool.QueueUserWorkItem(o =>
                            EstablishInterconnection(message.Get<InterconnectionValue>()));
                        break;
                    case CommonTags.RememberMe:
                        OnInterconnectionEstablished(message.Get<ThreadAddress>(),
                            remoteEndPoint);
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
                // Open a hole on my side.
                Dispatcher.Ping(endPoint, ReceiptTimeout);
                // I'm lucky! A hole is already opened.
                success = true;
            }
            catch (TimeoutException)
            {
                success = false;
            }
            stopwatch.Stop();
            timeElapsed = stopwatch.Elapsed;
            if (!success && ConnectingFailed != null)
            {
                ConnectingFailed(this, new EventArgs<IPEndPoint>(endPoint));
            }
            return success;
        }

        private bool EstablishInterconnection(InterconnectionValue interconnection)
        {
            if (InterconnectionEstablishing != null)
            {
                InterconnectionEstablishing(this, new EventArgs<InterconnectionValue>(
                    interconnection));
            }
            TimeSpan localTimeElapsed, externalTimeElapsed;
            bool localSucceeded = EstablishInterconnection(interconnection.LocalEndPoint,
                out localTimeElapsed);
            bool externalSucceeded = EstablishInterconnection(interconnection.ExternalEndPoint,
                out externalTimeElapsed);
            if (!localSucceeded && !externalSucceeded)
            {
                return false;
            }
            if (localSucceeded && !externalSucceeded)
            {
                OnInterconnectionEstablished(interconnection, interconnection.LocalEndPoint);
                return true;
            }
            if (!localSucceeded)
            {
                OnInterconnectionEstablished(interconnection, interconnection.ExternalEndPoint);
                return true;
            }
            OnInterconnectionEstablished(interconnection, 
                localTimeElapsed < externalTimeElapsed ? 
                interconnection.LocalEndPoint : interconnection.ExternalEndPoint);
            return true;
        }

        private void OnInterconnectionEstablished(ThreadAddress threadAddress,
            IPEndPoint remoteEndPoint)
        {
            neighbors[threadAddress] = remoteEndPoint;
            if (InterconnectionEstablished != null)
            {
                InterconnectionEstablished(this, new EventArgs<Tuple<ThreadAddress, IPEndPoint>>(
                    new Tuple<ThreadAddress, IPEndPoint>(threadAddress, remoteEndPoint)));
            }
        }

        private void OnInterconnectionEstablished(InterconnectionValue interconnection,
            IPEndPoint remoteEndPoint)
        {
            IPEndPoint storedEndPoint;
            if (neighbors.TryGetValue(interconnection.RemoteThreadAddress, out storedEndPoint) &&
                storedEndPoint.Equals(remoteEndPoint))
            {
                // Oh, we already have this address!
                return;
            }
            OnInterconnectionEstablished(interconnection.RemoteThreadAddress,
                remoteEndPoint);
            // Send info back to my neighbor. For now, a hole to it is opened.
            Dispatcher.Send(remoteEndPoint, interconnection.LocalThreadAddress, 
                CommonTags.RememberMe);
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
