using System;
using System.Net;
using System.Net.Sockets;
using Cloudy.Messaging.Interfaces;

namespace Cloudy.Helpers
{
    /// <summary>
    /// A helper class that adapts the <see cref="UdpClient"/> class
    /// to the <see cref="IRawCommunicator"/> interface.
    /// </summary>
    public class UdpClientRawCommunicator : IRawCommunicator<IPEndPoint>
    {
        private readonly UdpClient client;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="client">The underlying UDP client.</param>
        public UdpClientRawCommunicator(UdpClient client)
        {
            this.client = client;
        }

        #region Implementation of IRawCommunicator

        /// <summary>
        /// Sends a byte array.
        /// </summary>
        public void Send(byte[] data)
        {
            client.Send(data, data.Length);
        }

        /// <summary>
        /// Receives a data.
        /// </summary>
        /// <returns>Received data. Indicates the end of stream if empty array is returned.</returns>
        public byte[] Receive()
        {
            IPEndPoint remoteEndPoint = null;
            return client.Receive(ref remoteEndPoint);
        }

        /// <summary>
        /// Closes the sender-receiver.
        /// </summary>
        public void Close()
        {
            client.Close();
        }

        #endregion

        #region Implementation of IRawCommunicator<IPEndPoint>

        /// <summary>
        /// Sends a byte array.
        /// </summary>
        public void Send(byte[] data, IPEndPoint endPoint)
        {
            client.Send(data, data.Length, endPoint);
        }

        /// <summary>
        /// Receives a data.
        /// </summary>
        /// <returns>Received data. Indicates the end of stream if empty array is returned.</returns>
        public byte[] Receive(out IPEndPoint endPoint)
        {
            endPoint = null;
            return client.Receive(ref endPoint);
        }

        #endregion
    }
}
