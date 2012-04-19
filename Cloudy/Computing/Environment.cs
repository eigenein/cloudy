using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Cloudy.Collections;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Reduction;
using Cloudy.Computing.Structures;
using Cloudy.Computing.Structures.Values;
using Cloudy.Computing.Structures.Values.Environment;
using Cloudy.Computing.Topologies.Helpers;
using Cloudy.Computing.Topologies.Interfaces;
using Cloudy.Helpers;

namespace Cloudy.Computing
{
    /// <summary>
    /// Implements a computing thread environment methods.
    /// </summary>
    internal class Environment : IInternalEnvironment, IDisposable
    {
        protected Stopwatch Stopwatch = new Stopwatch();

        protected readonly IEnvironmentTransport Transport;

        protected readonly BlockingFilteredQueue<EnvironmentOperationValue> Queue =
            new BlockingFilteredQueue<EnvironmentOperationValue>();

        private readonly MemoryStorage<MemoryStorageObject> masterRemoteMemoryCache =
            new MemoryStorage<MemoryStorageObject>();

        private int operationId;

        private byte[] rawRank;

        public Environment(IEnvironmentTransport transport, byte[] rank)
        {
            this.Transport = transport;
            this.rawRank = rank;
            Stopwatch.Start();
        }

        public byte[] RawRank
        {
            get { return rawRank; }
            set
            {
                rawRank = value;
                if (RawRankChanged != null)
                {
                    RawRankChanged(this, new EventArgs<byte[]>(value));
                }
            }
        }

        public event ParameterizedEventHandler<byte[]> RawRankChanged;

        public void NotifyValueReceived(EnvironmentOperationValue value)
        {
            Queue.Enqueue(value);
        }

        public void CleanUp()
        {
            masterRemoteMemoryCache.CleanUp(
                value => value.TimeToLive != TimeToLive.Forever);
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

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Queue.Dispose();
            }
        }

        #endregion

        #region Implementation of IEnvironment

        public void SetRemoteValue<TValue>(byte[] @namespace, string key, 
            TValue value, TimeToLive timeToLive)
        {
            SetRemoteValueRequest request = new SetRemoteValueRequest();
            request.Namespace = @namespace;
            request.Key = key;
            request.Set(new WrappedValue<TValue>(value));
            request.TimeToLive = timeToLive;
            Transport.SendToMaster(request, Tags.SetRemoteValueRequest);
        }

        public bool TryGetRemoteValue<TValue>(byte[] @namespace, string key, out TValue value)
        {
            lock (masterRemoteMemoryCache)
            {
                MemoryStorageObject rawValue;
                if (masterRemoteMemoryCache.TryGetValue(@namespace, key, out rawValue))
                {
                    value = (TValue)rawValue.Value;
                    return true;
                }
                lock (Transport.MasterConversationLock)
                {
                    GetRemoteValueRequest request = new GetRemoteValueRequest();
                    request.Namespace = @namespace;
                    request.Key = key;
                    Transport.SendToMaster(request, Tags.GetRemoteValueRequest);
                    GetRemoteValueResponse response = Transport.ReceiveFromMaster<GetRemoteValueResponse>(
                        Tags.GetRemoteValueResponse);
                    if (response.Success != false)
                    {
                        value = response.Get<WrappedValue<TValue>>().Value;
                        if (response.TimeToLive != TimeToLive.Flash)
                        {
                            rawValue = new MemoryStorageObject();
                            rawValue.Value = value;
                            rawValue.TimeToLive = response.TimeToLive;
                            masterRemoteMemoryCache.Add(@namespace, key, rawValue);
                        }
                        return true;
                    }
                    value = default(TValue);
                    return false;
                }
            }
        }

        #endregion
    }

    internal class Environment<TRank> : Environment,
        IEnvironment<TRank> where TRank : IRank
    {
        private TRank rank;

        public Environment(IEnvironmentTransport transport, byte[] rank)
            : base(transport, rank)
        {
            this.rank = RankConverter<TRank>.Convert(rank);
            RawRankChanged += OnRawRankChanged;
        }

        public TRank Rank
        {
            get { return rank; }
        }

        private void OnRawRankChanged(object sender, EventArgs<byte[]> e)
        {
            rank = RankConverter<TRank>.Convert(e.Value);
        }

        #region Send

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
        public void Send<T>(int tag, T value, IEnumerable<TRank> recipients)
        {
            EnvironmentOperationValue operationValue = new EnvironmentOperationValue();
            operationValue.Sender = RawRank;
            operationValue.OperationId = GetOperationId();
            operationValue.OperationType = EnvironmentOperationType.PeerToPeer;
            operationValue.Set(new WrappedValue<T>(value));
            operationValue.Recipients = recipients.Select(RankConverter<TRank>.Convert).ToList();
            Transport.Send(operationValue);
        }

        #endregion

        #region Receive

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

        #endregion

        #region Reduction

        /// <summary>
        /// Performs the reduction operation. It combines the values provided 
        /// by each thread, using a specified <paramref name="operation"/>, 
        /// and returns the combined value.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="tag">The user tag.</param>
        /// <param name="operation">The reduce operation.</param>
        /// <param name="targets">Threads to gather values from.</param>
        /// <returns>The combined value.</returns>
        public T Reduce<T>(int tag, ReduceOperation operation, IEnumerable<TRank> targets)
        {
            // Prepare the request.
            EnvironmentOperationValue requestOperationValue = new EnvironmentOperationValue();
            requestOperationValue.Recipients = targets.Select(RankConverter<TRank>.Convert).ToList();
            if (requestOperationValue.Recipients.Count == 0)
            {
                return default(T);
            }
            requestOperationValue.OperationId = GetOperationId();
            requestOperationValue.OperationType = EnvironmentOperationType.ReduceRequest;
            requestOperationValue.Sender = RawRank;
            requestOperationValue.UserTag = tag;
            ReduceRequestValue value = new ReduceRequestValue();
            value.Participants = requestOperationValue.Recipients;
            value.Operation = operation;
            requestOperationValue.Set(value);
            // Send the request.
            Transport.Send(requestOperationValue);
            // Awaiting for the response.
            EnvironmentOperationValue previousOperationValue = Queue.Dequeue(v =>
                v.OperationId == requestOperationValue.OperationId &&
                v.OperationType == EnvironmentOperationType.ReduceResponse);
            return previousOperationValue.Get<WrappedValue<T>>().Value;
        }

        /// <summary>
        /// Performs the reduction operation. It combines the values provided 
        /// by each thread, using a specified <paramref name="operation"/>, 
        /// and returns the combined value.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="tag">The user tag.</param>
        /// <param name="value">A value that is provided by the local node.</param>
        /// <param name="operation">The reduce operation.</param>
        /// <param name="targets">Threads to gather values from.</param>
        /// <returns>The combined value.</returns>
        public T Reduce<T>(int tag, T value, ReduceOperation operation, IEnumerable<TRank> targets)
        {
            return ReduceHelper<T>.Reduce(value, Reduce<T>(tag, operation, targets), operation);
        }

        /// <summary>
        /// Provides a value for a reduction operation.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="tag">The user tag.</param>
        /// <param name="value">The value to combine.</param>
        public void Reduce<T>(int tag, T value)
        {
            EnvironmentOperationValue operationValue = Queue.Dequeue(v =>
                v.OperationType == EnvironmentOperationType.ReduceRequest && v.UserTag == tag);
            Reduce(value, operationValue);
        }

        /// <summary>
        /// Provides a value for a reduction operation.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="tag">The user tag.</param>
        /// <param name="value">The value to combine.</param>
        /// <param name="sender">The rank of a node that should request the reduction operation.</param>
        public void Reduce<T>(int tag, T value, TRank sender)
        {
            byte[] rawSenderRank = RankConverter<TRank>.Convert(sender);
            EnvironmentOperationValue operationValue = Queue.Dequeue(v =>
                v.UserTag == tag && v.Sender.SameAs(rawSenderRank) && 
                    v.OperationType == EnvironmentOperationType.ReduceRequest);
            Reduce(value, operationValue);
        }

        /// <summary>
        /// Handles the reduce request.
        /// </summary>
        private void Reduce<T>(T value, EnvironmentOperationValue requestOperationValue)
        {
            ReduceRequestValue request = requestOperationValue.Get<ReduceRequestValue>();
            // Initialize reduced value...
            T result = value;
            // Okay. Now we're to decide whether we should wait for another node...
            byte[] previousRank = null;
            IEnumerator<byte[]> participantsEnumerator = request.Participants.GetEnumerator();
            while (participantsEnumerator.MoveNext())
            {
                if (participantsEnumerator.Current.SameAs(RawRank))
                {
                    break;
                }
                previousRank = participantsEnumerator.Current;
            }
            if (previousRank != null)
            {
                // Wait the previous node.
                EnvironmentOperationValue previousOperationValue = Queue.Dequeue(v =>
                    v.OperationId == requestOperationValue.OperationId &&
                    v.OperationType == EnvironmentOperationType.ReduceResponse &&
                    v.Sender.SameAs(previousRank));
                T previousValue = previousOperationValue.Get<WrappedValue<T>>().Value;
                result = ReduceHelper<T>.Reduce(result, previousValue, request.Operation);
            }
            // Prepare response and send it.
            EnvironmentOperationValue responseOperationValue = new EnvironmentOperationValue();
            responseOperationValue.OperationId = requestOperationValue.OperationId;
            responseOperationValue.OperationType = EnvironmentOperationType.ReduceResponse;
            responseOperationValue.Recipients = new[] { participantsEnumerator.MoveNext() ? 
                participantsEnumerator.Current : requestOperationValue.Sender };
            responseOperationValue.Sender = RawRank;
            responseOperationValue.UserTag = requestOperationValue.UserTag;
            responseOperationValue.Set(new WrappedValue<T>(result));
            Transport.Send(responseOperationValue);
        }

        #endregion

        public double GetTime()
        {
            return Stopwatch.Elapsed.TotalSeconds;
        }
    }
}
