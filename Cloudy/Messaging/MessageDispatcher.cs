using System;
using System.Collections.Generic;
using System.Threading;
using Cloudy.Collections;
using Cloudy.Helpers;
using Cloudy.Messaging.Enums;
using Cloudy.Messaging.Interfaces;
using Cloudy.Messaging.Structures;
using Cloudy.Structures;
using EmptyValue = Cloudy.Messaging.Structures.EmptyValue;

namespace Cloudy.Messaging
{
    /// <summary>
    /// Sends, receives and tracks messages.
    /// Sequential receiving, asynchronous sending.
    /// </summary>
    public class MessageDispatcher<TEndPoint> : IDisposable
    {
        #region Private Fields

        private readonly Communicator<TEndPoint> communicator;

        private readonly Dictionary<long, MessagingAsyncResult> sendQueue =
            new Dictionary<long, MessagingAsyncResult>();

        private readonly BlockingQueue<Tuple<TrackableDto, TEndPoint>> receiveQueue = 
            new BlockingQueue<Tuple<TrackableDto, TEndPoint>>();

        #endregion

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="communicator">The underlying message stream.</param>
        public MessageDispatcher(Communicator<TEndPoint> communicator)
        {
            this.communicator = communicator;
            this.SendAttemptsCount = 2;
        }

        #region ID creating

        private long nextTrackingId;

        /// <summary>
        /// Creates a new tracking ID.
        /// </summary>
        private long CreateTrackingId()
        {
            return Interlocked.Increment(ref nextTrackingId);
        }

        #endregion

        #region Properties

        public int SendAttemptsCount { get; set; }

        /// <summary>
        /// Gets the underlying input message stream.
        /// </summary>
        public Communicator<TEndPoint> Communicator
        {
            get { return communicator; }
        }

        /// <summary>
        /// Gets the count of already received and buffered messages.
        /// </summary>
        public int Available
        {
            get
            {
                return receiveQueue.Count;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Processes incoming messages.
        /// </summary>
        /// <param name="count">The count of messages to be processed.</param>
        /// <returns>The count of messages actually processed.</returns>
        public int ProcessIncomingMessages(int count)
        {
            int processedMessagesCount = 0;
            // Loop through messages.
            while (processedMessagesCount < count)
            {
                TEndPoint remoteEndPoint;
                TrackableDto message = communicator.Receive<TrackableDto>(out remoteEndPoint);
                if (message.Tag == CommonTags.Receipt)
                {
                    MessagingAsyncResult ar;
                    if (sendQueue.TryGetValue(message.TrackingId, out ar))
                    {
                        ar.Notify();
                        if (ar.IsCompleted)
                        {
                            sendQueue.Remove(message.TrackingId);
                            ar.Dispose();
                        }
                    }
                }
                else
                {
                    // Regular message. Send the delivery notification.
                    communicator.Send(new TrackableDto(message.TrackingId,
                        CommonTags.Receipt, null), remoteEndPoint);
                    if (message.Tag != CommonTags.Ping)
                    {
                        receiveQueue.Enqueue(new Tuple<TrackableDto, TEndPoint>(
                            message, remoteEndPoint));
                    }
                }
                processedMessagesCount++;
            }
            return processedMessagesCount;
        }

        /// <summary>
        /// Starts an asynchronous sending of the message.
        /// </summary>
        public MessagingAsyncResult BeginSend<T>(TEndPoint endPoint,
            T message, int tag, AsyncCallback callback, object state)
        {
            TrackableDto<T> dto = new TrackableDto<T>(CreateTrackingId(), tag, message);
            MessagingAsyncResult ar = new MessagingAsyncResult(
                dto.TrackingId, 1, callback, state);
            sendQueue.Add(dto.TrackingId, ar);
            communicator.Send(dto, endPoint);
            return ar;
        }

        /// <summary>
        /// Starts an asynchronous sending of the message.
        /// </summary>
        public MessagingAsyncResult BeginSend<T>(TEndPoint[] endPoints, 
            T message, int tag, AsyncCallback callback, object state)
        {
            long trackingId = CreateTrackingId();
            MessagingAsyncResult ar = new MessagingAsyncResult(
                trackingId, endPoints.Length, callback, state);
            sendQueue.Add(trackingId, ar);
            // Pre-serialize the DTO to improve performance.
            byte[] bytes = new TrackableDto<T>(trackingId, tag, message).Serialize();
            foreach (TEndPoint endPoint in endPoints)
            {
                communicator.SimpleCommunicator.Send(bytes, endPoint);
            }
            return ar;
        }

        /// <summary>
        /// Finishes an asynchronous sending of the message.
        /// </summary>
        public void EndSend(MessagingAsyncResult ar, TimeSpan timeout)
        {
            try
            {
                ar.EndInvoke(timeout);
            }
            catch (TimeoutException)
            {
                sendQueue.Remove(ar.TrackingId);
                ar.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Sends the message asynchronously, but without tracking.
        /// </summary>
        public void SendAsync<T>(TEndPoint endPoint, T message, int tag)
        {
            TrackableDto<T> dto = new TrackableDto<T>(CreateTrackingId(), tag, message);
            communicator.Send(dto, endPoint);
        }

        /// <summary>
        /// Sends the message synchronously. This will not pause sending and receiving
        /// of other messages until returned.
        /// </summary>
        public void Send<T>(TEndPoint endPoint, T message, int tag, TimeSpan timeout)
        {
            Exception innerException = null;
            for (int i = 0; i < SendAttemptsCount; i++)
            {
                MessagingAsyncResult ar = BeginSend(endPoint, message, tag, null, null);
                try
                {
                    EndSend(ar, timeout);
                    return;
                }
                catch (TimeoutException ex)
                {
                    innerException = ex;
                }
            }
            throw new TimeoutException(String.Format("Timeout: {0}", timeout),
                innerException);
        }

        /// <summary>
        /// Starts an asynchronous ping.
        /// </summary>
        public MessagingAsyncResult BeginPing(TEndPoint endPoint,
            AsyncCallback callback, object state)
        {
            return BeginSend(endPoint, EmptyValue.Instance, CommonTags.Ping,
                callback, state);
        }

        /// <summary>
        /// Starts an asynchronous ping.
        /// </summary>
        public MessagingAsyncResult BeginPing(TEndPoint[] endPoints,
            AsyncCallback callback, object state)
        {
            return BeginSend(endPoints, EmptyValue.Instance, CommonTags.Ping,
                callback, state);
        }

        /// <summary>
        /// Finishes an asynchronous ping.
        /// </summary>
        public void EndPing(MessagingAsyncResult ar, TimeSpan timeout)
        {
            ar.EndInvoke(timeout);
        }

        /// <summary>
        /// Pings the endpoint synchronously. This will not pause sending
        /// and receiving of other messages until returned.
        /// </summary>
        public void Ping(TEndPoint endPoint, TimeSpan timeout)
        {
            Send(endPoint, EmptyValue.Instance, CommonTags.Ping, timeout);
        }

        /// <summary>
        /// Receives a message.
        /// </summary>
        public IMessage Receive(out TEndPoint remoteEndPoint, out int tag)
        {
            return Receive(out remoteEndPoint, out tag, TimeSpanExtensions.Infinite);
        }

        /// <summary>
        /// Receives a message.
        /// </summary>
        public TResult Receive<TResult>()
        {
            TEndPoint remoteEndPoint;
            int tag;
            return Receive<TResult>(out remoteEndPoint, out tag, TimeSpanExtensions.Infinite);
        }

        /// <summary>
        /// Receives a message.
        /// </summary>
        public TResult Receive<TResult>(TimeSpan timeout)
        {
            TEndPoint remoteEndPoint;
            int tag;
            return Receive<TResult>(out remoteEndPoint, out tag, timeout);
        }

        /// <summary>
        /// Receives a message.
        /// </summary>
        public TResult Receive<TResult>(out TEndPoint remoteEndPoint, out int tag)
        {
            return Receive<TResult>(out remoteEndPoint, out tag, TimeSpanExtensions.Infinite);
        }

        /// <summary>
        /// Receives a message.
        /// </summary>
        public IMessage Receive(out TEndPoint remoteEndPoint, out int tag,
            TimeSpan timeout)
        {
            while (true)
            {
                Tuple<TrackableDto, TEndPoint> pair = receiveQueue.Dequeue(timeout);
                tag = pair.Item1.Tag;
                remoteEndPoint = pair.Item2;
                return pair.Item1;
            }
        }

        /// <summary>
        /// Receives a message.
        /// </summary>
        public TResult Receive<TResult>(out TEndPoint remoteEndPoint, out int tag,
            TimeSpan timeout)
        {
            return Receive(out remoteEndPoint, out tag, timeout).Get<TResult>();
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, 
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
                communicator.Dispose();
                receiveQueue.Dispose();
            }
        }
    }
}
