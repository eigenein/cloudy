using System;
using Cloudy.Computing.Structures.Values;

namespace Cloudy.Computing.Interfaces
{
    internal interface IEnvironmentTransport
    {
        /// <summary>
        /// Use this lock every time you need a bi-directional conversation
        /// with the Master using both Send and Receive methods. One-directional
        /// replies and notifications don't need this lock.
        /// </summary>
        object MasterConversationLock { get; }

        void SendToMaster<TMessage>(TMessage message, int tag);

        TMessage ReceiveFromMaster<TMessage>(int expectedTag);

        /// <summary>
        /// Sends the operation value.
        /// </summary>
        /// <remarks>
        /// The assigned recipients collection will be altered during this 
        /// operation.
        /// </remarks>
        void Send(EnvironmentOperationValue operationValue);
    }
}
