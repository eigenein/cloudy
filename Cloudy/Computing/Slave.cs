using System;
using System.Collections.Generic;
using System.Net;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Events;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Messaging.Structures;
using Cloudy.Computing.Topologies.Structures;
using Cloudy.Messaging.Enums;
using Cloudy.Messaging.Interfaces;
using Cloudy.Messaging.Structures;

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

        private SlaveState state = SlaveState.NotJoined;

        protected Slave(IPEndPoint localEndPoint) 
            : base(localEndPoint.Port)
        {
            this.localEndPoint = localEndPoint;
        }

        public abstract int SlotsCount { get; }

        public SlaveState State
        {
            get { return state; }
        }

        /// <summary>
        /// Occurs when a slaves successfully joins the network.
        /// </summary>
        public event EventHandler<JoinedEventArgs> Joined;

        /// <summary>
        /// Occurs when a thread is allocated in the slave.
        /// </summary>
        public event EventHandler<ThreadAllocatedEventArgs> ThreadAllocated;

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
            MessagingAsyncResult ar = Dispatcher.BeginSend(remoteEndPoint, 
                new JoinRequestValue(localEndPoint, SlotsCount, metadata), 
                CommonTags.JoinRequest, null, null);
            Dispatcher.EndSend(ar, ReceiptTimeout);
            JoinResponseValue response = Dispatcher.Receive<JoinResponseValue>(
                ResponseTimeout);
            state = SlaveState.Joined;
            if (Joined != null)
            {
                Joined(this, new JoinedEventArgs(response.ExternalEndPoint));
            }
        }

        public override int ProcessIncomingMessages(int count)
        {
            int processedMessagesCount = Dispatcher.ProcessIncomingMessages(count);
            while (state == SlaveState.Joined && Dispatcher.Available > 0)
            {
                int? tag;
                IPEndPoint remoteEndPoint;
                ICastable message = Dispatcher.Receive(out remoteEndPoint, out tag);
                switch (tag)
                {
                    case CommonTags.AllocateThread:
                        AllocateThreadValue value = message.Cast<AllocateThreadValue>();
                        if (ThreadAllocated != null)
                        {
                            ThreadAllocated(this, new ThreadAllocatedEventArgs(value.ThreadAddress));
                        }
                        break;
                }
            }
            return processedMessagesCount;
        }
    }
}
