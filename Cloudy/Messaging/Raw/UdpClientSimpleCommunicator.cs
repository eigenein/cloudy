using System;
using System.Net;
using System.Net.Sockets;
using Cloudy.Messaging.Interfaces;

namespace Cloudy.Messaging.Raw
{
    /// <summary>
    /// A helper class that adapts the <see cref="UdpClient"/> class
    /// to the <see cref="ISimpleCommunicator"/> interface.
    /// </summary>
    public class UdpClientSimpleCommunicator : ISimpleCommunicator<IPEndPoint>
    {
        private readonly UdpClient udpClient;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="udpClient">The underlying UDP client.</param>
        public UdpClientSimpleCommunicator(UdpClient udpClient)
        {
            this.udpClient = udpClient;
        }

        #region Implementation of IRawCommunicator

        /// <summary>
        /// Sends a byte array.
        /// </summary>
        public void Send(byte[] data)
        {
            udpClient.Send(data, data.Length);
        }

        /// <summary>
        /// Receives a data.
        /// </summary>
        /// <returns>Received data. Indicates the end of stream if empty array is returned.</returns>
        public byte[] Receive()
        {
            IPEndPoint remoteEndPoint = null;
            return udpClient.Receive(ref remoteEndPoint);
        }

        /// <summary>
        /// Closes the sender-receiver.
        /// </summary>
        public void Close()
        {
            udpClient.Close();
        }

        #endregion

        #region Implementation of IRawCommunicator<IPEndPoint>

        /// <summary>
        /// Sends a byte array.
        /// </summary>
        public void Send(byte[] data, IPEndPoint endPoint)
        {
            udpClient.Send(data, data.Length, endPoint);
        }

        /// <summary>
        /// Receives a data.
        /// </summary>
        /// <returns>Received data. Indicates the end of stream if empty array is returned.</returns>
        public byte[] Receive(out IPEndPoint endPoint)
        {
            endPoint = null;
            return udpClient.Receive(ref endPoint);
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                udpClient.Close();
            }
        }
    }
}
