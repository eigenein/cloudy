using System;
using System.Net;
using System.Net.Sockets;
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

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="port">The source UDP port.</param>
        protected Node(int port)
        {
            UdpClient udpClient = new UdpClient(port);
            IRawCommunicator<IPEndPoint> rawCommunicator =
                new UdpClientRawCommunicator(udpClient);
            Communicator<IPEndPoint> communicator =
                new Communicator<IPEndPoint>(rawCommunicator);
            this.Dispatcher = new MessageDispatcher<IPEndPoint>(communicator);
        }

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
