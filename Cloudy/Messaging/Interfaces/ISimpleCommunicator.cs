﻿using System;

namespace Cloudy.Messaging.Interfaces
{
    /// <summary>
    /// Provides a generic interface to any entity that is able to send and receive.
    /// </summary>
    public interface ISimpleCommunicator : IDisposable
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
    }

    /// <summary>
    /// Provides a generic interface to any entity that is able to send and receive.
    /// </summary>
    /// <typeparam name="TEndPoint">The endpoint type.</typeparam>
    public interface ISimpleCommunicator<TEndPoint> : ISimpleCommunicator
    {
        /// <summary>
        /// Sends a byte array.
        /// </summary>
        void Send(byte[] data, TEndPoint endPoint);

        /// <summary>
        /// Receives a data.
        /// </summary>
        /// <returns>Received data. Indicates the end of stream if empty array is returned.</returns>
        byte[] Receive(out TEndPoint endPoint);
    }
}
