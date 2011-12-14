using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cloudy.Collections;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Structures.Values;
using Cloudy.Computing.Topologies.Interfaces;
using Cloudy.Helpers;
using Cloudy.Protobuf;

namespace Cloudy.Computing
{
    /// <summary>
    /// Implements a computing thread environment methods.
    /// </summary>
    internal class Environment : IInternalEnvironment, IDisposable
    {
        protected readonly IEnvironmentTransport Transport;

        protected readonly BlockingFilteredQueue<EnvironmentOperationValue> Queue =
            new BlockingFilteredQueue<EnvironmentOperationValue>();

        protected readonly byte[] RawRank;

        private int operationId;

        public Environment(IEnvironmentTransport transport, byte[] rank)
        {
            this.Transport = transport;
            this.RawRank = rank;
        }

        public void NotifyValueReceived(EnvironmentOperationValue value)
        {
            Queue.Enqueue(value);
        }

        protected int GetOperationId()
        {
            return Interlocked.Increment(ref operationId);
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
                Queue.Dispose();
            }
        }
    }

    internal class Environment<TRank> : Environment,
        IEnvironment<TRank> where TRank : IRank
    {
        private readonly TRank rank;

        public Environment(IEnvironmentTransport transport, byte[] rank)
            : base(transport, rank)
        {
            this.rank = RankConverter<TRank>.Convert(rank);
        }

        public TRank Rank
        {
            get { return rank; }
        }

        /// <summary>
        /// Performs a blocking send.
        /// </summary>
        public void Send<T>(int tag, T value, TRank recipient)
        {
            Send(tag, value, new[] { recipient });
        }

        /// <summary>
        /// Performs a blocking send.
        /// </summary>
        public void Send<T>(int tag, T value, ICollection<TRank> recipients)
        {
            EnvironmentOperationValue operationValue = new EnvironmentOperationValue();
            operationValue.Sender = RawRank;
            operationValue.OperationId = GetOperationId();
            operationValue.OperationType = EnvironmentOperationType.PeerToPeer;
            operationValue.Set(new WrappedValue<T>(value));
            operationValue.Recipients = recipients.Select(RankConverter<TRank>.Convert).ToList();
            Transport.Send(operationValue);
        }

        /// <summary>
        /// Blocking receive for a message
        /// </summary>
        public void Receive<T>(int tag, out T value, out TRank sender)
        {
            EnvironmentOperationValue operationValue = Queue.Dequeue(
                v => v.UserTag == tag && v.OperationType == EnvironmentOperationType.PeerToPeer);
            value = operationValue.Get<WrappedValue<T>>().Value;
            sender = RankConverter<TRank>.Convert(operationValue.Sender);
        }

        /// <summary>
        /// Blocking receive for a message
        /// </summary>
        public void Receive<T>(out int tag, out T value, out TRank sender)
        {
            EnvironmentOperationValue operationValue = Queue.Dequeue(
                v => v.OperationType == EnvironmentOperationType.PeerToPeer);
            value = operationValue.Get<WrappedValue<T>>().Value;
            sender = RankConverter<TRank>.Convert(operationValue.Sender);
            tag = operationValue.UserTag;
        }

        /// <summary>
        /// Blocking receive for a message
        /// </summary>
        public void Receive<T>(int tag, out T value, TRank sender)
        {
            byte[] rawSenderRank = RankConverter<TRank>.Convert(sender);
            EnvironmentOperationValue operationValue = Queue.Dequeue(
                v => v.Sender.SameAs(rawSenderRank) && v.UserTag == tag &&
                    v.OperationType == EnvironmentOperationType.PeerToPeer);
            value = operationValue.Get<WrappedValue<T>>().Value;
        }

        /// <summary>
        /// Blocking receive for a message
        /// </summary>
        public void Receive<T>(out int tag, out T value, TRank sender)
        {
            byte[] rawSenderRank = RankConverter<TRank>.Convert(sender);
            EnvironmentOperationValue operationValue = Queue.Dequeue(
                v => v.Sender.SameAs(rawSenderRank) &&
                    v.OperationType == EnvironmentOperationType.PeerToPeer);
            value = operationValue.Get<WrappedValue<T>>().Value;
            tag = operationValue.UserTag;
        }
    }
}
