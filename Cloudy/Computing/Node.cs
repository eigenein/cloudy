using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Cloudy.Computing.Topologies.Structures;
using Cloudy.Messaging;
using Cloudy.Messaging.Interfaces;
using Cloudy.Messaging.Raw;

namespace Cloudy.Computing
{
    /// <summary>
    /// Represents an abstract node and implements common logic for both
    /// master and slave.
    /// </summary>
    public abstract class Node : IDisposable
    {
        protected readonly MessageDispatcher<IPEndPoint> Dispatcher;

        protected readonly Dictionary<ThreadAddress, IPEndPoint> Neighbors =
            new Dictionary<ThreadAddress, IPEndPoint>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="port">The source UDP port.</param>
        protected Node(int port)
        {
            this.PingTimeout = new TimeSpan(0, 0, 1);

            IRawCommunicator<IPEndPoint> rawCommunicator =
                new UdpClientRawCommunicator(new UdpClient(port));
            this.Dispatcher = new MessageDispatcher<IPEndPoint>(
                new Communicator<IPEndPoint>(rawCommunicator));
        }

        /// <summary>
        /// Gets or sets the timeout for delivery notification when sending 
        /// a message.
        /// </summary>
        public TimeSpan PingTimeout { get; set; }

        protected abstract int ProcessIncomingMessages(int count);

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
                Dispatcher.Dispose();
            }
        }
    }
}
