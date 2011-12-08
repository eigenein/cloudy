using System;
using System.Collections.Generic;
using System.Threading;
using Cloudy.Collections;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Structures.Values;

namespace Cloudy.Computing
{
    /// <summary>
    /// A default implementation of the <see cref="IEnvironment"/> interface.
    /// </summary>
    internal class Environment : IEnvironment
    {
        private readonly Guid threadId;

        private readonly IEnvironmentTransport transport;

        private readonly BlockingFilteredQueue<EnvironmentOperationValue> queue =
            new BlockingFilteredQueue<EnvironmentOperationValue>();

        private int operationId;

        public Environment(IEnvironmentTransport transport, Guid threadId)
        {
            this.transport = transport;
            this.threadId = threadId;
        }

        public Guid ThreadId
        {
            get { return threadId; }
        }

        public void NotifyValueReceived(EnvironmentOperationValue value)
        {
            queue.Enqueue(value);
        }

        private int GetOperationId()
        {
            return Interlocked.Increment(ref operationId);
        }

        /// <summary>
        /// Performs a blocking send.
        /// </summary>
        public void Send<T>(Guid recipientId, int tag, T value)
        {
            Send(tag, value, new[] { recipientId });
        }

        /// <summary>
        /// Performs a blocking send.
        /// </summary>
        public void Send<T>(int tag, T value, ICollection<Guid> recipientsIds)
        {
            EnvironmentOperationValue operationValue = new EnvironmentOperationValue();
            operationValue.SenderId = threadId;
            operationValue.OperationId = GetOperationId();
            operationValue.OperationType = EnvironmentOperationType.PeerToPeer;
            operationValue.UserTag = tag;
            operationValue.RecipientsIds = recipientsIds;
            operationValue.Set(value);
            throw new NotImplementedException("transport.Send is not available.");
            // TODO: transport.Send(recipientsIds, operationValue);
        }

        /// <summary>
        /// Blocking receive for a message
        /// </summary>
        public void Receive<T>(int tag, out T value, out Guid senderId)
        {
            EnvironmentOperationValue operationValue = queue.Dequeue(
                v => v.UserTag == tag && v.OperationType == EnvironmentOperationType.PeerToPeer);
            value = operationValue.Get<T>();
            senderId = operationValue.SenderId;
        }

        /// <summary>
        /// Blocking receive for a message
        /// </summary>
        public void Receive<T>(out int tag, out T value, out Guid senderId)
        {
            EnvironmentOperationValue operationValue = queue.Dequeue(
                v => v.OperationType == EnvironmentOperationType.PeerToPeer);
            value = operationValue.Get<T>();
            senderId = operationValue.SenderId;
            tag = operationValue.UserTag;
        }

        /// <summary>
        /// Blocking receive for a message
        /// </summary>
        public void Receive<T>(int tag, out T value, Guid senderId)
        {
            EnvironmentOperationValue operationValue = queue.Dequeue(
                v =>v.SenderId == senderId && v.UserTag == tag && 
                    v.OperationType == EnvironmentOperationType.PeerToPeer);
            value = operationValue.Get<T>();
        }

        /// <summary>
        /// Blocking receive for a message
        /// </summary>
        public void Receive<T>(out int tag, out T value, Guid senderId)
        {
            EnvironmentOperationValue operationValue = queue.Dequeue(
                v => v.SenderId == senderId && 
                    v.OperationType == EnvironmentOperationType.PeerToPeer);
            value = operationValue.Get<T>();
            tag = operationValue.UserTag;
        }
    }
}
