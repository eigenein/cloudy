using System;

namespace Cloudy.Messaging.Interfaces
{
    /// <summary>
    /// Provides a generic interface to any entity that is able to send and receive.
    /// </summary>
    public interface ISenderReceiver
    {
        /// <summary>
        /// Sends a byte array.
        /// </summary>
        void Send(byte[] data);

        /// <summary>
        /// Receives a data.
        /// </summary>
        /// <returns>Received data. Indicates the end of stream if empty array is returned.</returns>
        byte[] Receive();

        /// <summary>
        /// Closes the sender-receiver.
        /// </summary>
        void Close();
    }
}
