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
    internal class Environment : IInternalEnvironment, IDisposable
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

        public ICollection<Guid> ResolveId(Guid id)
        {
            return transport.ResolveId(threadId, id);
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
        public void Send<T>(int tag, T value, Guid recipientId)
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
            operationValue.Set(new WrappedValue<T>(value));
            transport.Send(operationValue);
        }

        /// <summary>
        /// Blocking receive for a message
        /// </summary>
        public void Receive<T>(int tag, out T value, out Guid senderId)
        {
            EnvironmentOperationValue operationValue = queue.Dequeue(
                v => v.UserTag == tag && v.OperationType == EnvironmentOperationType.PeerToPeer);
            value = operationValue.Get<WrappedValue<T>>().Value;
            senderId = operationValue.SenderId;
        }

        /// <summary>
        /// Blocking receive for a message
        /// </summary>
        public void Receive<T>(out int tag, out T value, out Guid senderId)
        {
            EnvironmentOperationValue operationValue = queue.Dequeue(
                v => v.OperationType == EnvironmentOperationType.PeerToPeer);
            value = operationValue.Get<WrappedValue<T>>().Value;
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
            value = operationValue.Get<WrappedValue<T>>().Value;
        }

        /// <summary>
        /// Blocking receive for a message
        /// </summary>
        public void Receive<T>(out int tag, out T value, Guid senderId)
        {
            EnvironmentOperationValue operationValue = queue.Dequeue(
                v => v.SenderId == senderId && 
                    v.OperationType == EnvironmentOperationType.PeerToPeer);
            value = operationValue.Get<WrappedValue<T>>().Value;
            tag = operationValue.UserTag;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                queue.Dispose();
            }
        }
    }
}
