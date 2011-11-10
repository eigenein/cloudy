using System;
using System.Net;
using System.Net.Sockets;
using Cloudy.Messaging.Interfaces;

namespace Cloudy.Helpers
{
    /// <summary>
    /// A helper class that adapts the <see cref="UdpClient"/> class
    /// to the <see cref="ISenderReceiver"/> interface.
    /// </summary>
    public class UdpClientSenderReceiver : ISenderReceiver
    {
        private readonly UdpClient client;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="client">The underlying UDP client.</param>
        public UdpClientSenderReceiver(UdpClient client)
        {
            this.client = client;
        }

        #region Implementation of ISenderReceiver

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
    }
}
