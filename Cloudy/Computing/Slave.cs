using System;
using System.Net;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Events;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Messaging.Structures;
using Cloudy.Messaging.Enums;
using Cloudy.Messaging.Structures;

namespace Cloudy.Computing
{
    /// <summary>
    /// Represents an abstract slave node.
    /// </summary>
    public abstract class Slave : Node
    {
        private readonly IPEndPoint localEndPoint;

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
            switch (state)
            {
                case SlaveState.Joined:
                    while (Dispatcher.Available > 0)
                    {
                        int? tag;
                        IPEndPoint remoteEndPoint;
                        // ICastable message = 
                            Dispatcher.Receive(out remoteEndPoint, out tag);
                    }
                    break;
            }
            return processedMessagesCount;
        }
    }
}
