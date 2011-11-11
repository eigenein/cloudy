using System;
using System.Collections.Generic;
using System.Threading;
using Cloudy.Helpers;
using Cloudy.Messaging.Delegates;
using Cloudy.Messaging.Enums;
using Cloudy.Messaging.Events;
using Cloudy.Messaging.Exceptions;
using Cloudy.Messaging.Interfaces;
using Cloudy.Messaging.Structures;

namespace Cloudy.Messaging
{
    /// <summary>
    /// Sends, receives and tracks messages.
    /// </summary>
    public class MessageDispatcher<TEndPoint> : IDisposable
    {
        #region Private Fields

        private readonly Communicator<TEndPoint> communicator;

        private readonly Guid fromId;

        private readonly ResolveEndPointDelegate<TEndPoint> resolveEndPoint;

        private readonly Dictionary<long, MessagingAsyncResult> sendQueue =
            new Dictionary<long, MessagingAsyncResult>();

        private readonly BlockingQueue<TrackableDto> receiveQueue = 
            new BlockingQueue<TrackableDto>();

        #endregion

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="resolveEndPoint">Used to find a stream by a recipient identifier.</param>
        /// <param name="communicator">The underlying message stream.</param>
        /// <param name="fromId">The sender identifier.</param>
        public MessageDispatcher(Guid fromId, ResolveEndPointDelegate<TEndPoint> resolveEndPoint,
            Communicator<TEndPoint> communicator)
        {
            this.communicator = communicator;
            this.resolveEndPoint = resolveEndPoint;
            this.fromId = fromId;
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

        /// <summary>
        /// Gets the underlying input message stream.
        /// </summary>
        public Communicator Communicator
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
                TEndPoint resolvedToEndPoint;
                TEndPoint remoteEndPoint;
                TrackableDto message = communicator.Receive<TrackableDto>(out remoteEndPoint);
                if (!resolveEndPoint(message.FromId, out resolvedToEndPoint) ||
                    !remoteEndPoint.Equals(resolvedToEndPoint))
                {
                    OnEndPointMismatched(message.FromId, resolvedToEndPoint);
                }
                else if (message.Tag == WellKnownTags.DeliveryNotification)
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
                    communicator.Send(new TrackableDto(fromId, message.TrackingId,
                        WellKnownTags.DeliveryNotification, null), remoteEndPoint);
                    if (message.Tag != WellKnownTags.Ping)
                    {
                        receiveQueue.Enqueue(message);
                    }
                }
                processedMessagesCount++;
            }
            return processedMessagesCount;
        }

        /// <summary>
        /// Starts an asynchronous sending of the message.
        /// </summary>
        public MessagingAsyncResult BeginSend<T>(Guid recipient,
            T message, int? tag, AsyncCallback callback, object state)
        {
            TEndPoint endPoint;
            if (!resolveEndPoint(recipient, out endPoint))
            {
                throw new EndPointUnresolvedException(recipient);
            }
            TrackableDto<T> dto = new TrackableDto<T>(fromId, CreateTrackingId(), tag, message);
            communicator.Send(dto, endPoint);
            MessagingAsyncResult ar = new MessagingAsyncResult(1, callback, state);
            sendQueue.Add(dto.TrackingId, ar);
            return ar;
        }

        /// <summary>
        /// Starts an asynchronous sending of the message.
        /// </summary>
        public MessagingAsyncResult BeginSend<T>(Guid[] recipients, 
            T message, int? tag, AsyncCallback callback, object state)
        {
            // Pre-serialize the DTO's value to improve performance.
            long trackingId = CreateTrackingId();
            TrackableDto dto = new TrackableDto<T>(
                fromId, trackingId, tag, message).Preserialize();
            foreach (Guid recipient in recipients)
            {
                TEndPoint endPoint;
                if (!resolveEndPoint(recipient, out endPoint))
                {
                    throw new EndPointUnresolvedException(recipient);
                }
                communicator.Send(dto, endPoint);
            }
            MessagingAsyncResult ar = new MessagingAsyncResult(recipients.Length,
                callback, state);
            sendQueue.Add(trackingId, ar);
            return ar;
        }

        /// <summary>
        /// Finishes an asynchronous sending of the message.
        /// </summary>
        public void EndSend(MessagingAsyncResult ar, TimeSpan timeout)
        {
            ar.EndInvoke(timeout);
        }

        /// <summary>
        /// Starts an asynchronous ping.
        /// </summary>
        public MessagingAsyncResult BeginPing(Guid target,
            AsyncCallback callback, object state)
        {
            return BeginSend<object>(target, null, WellKnownTags.Ping,
                callback, state);
        }

        /// <summary>
        /// Starts an asynchronous ping.
        /// </summary>
        public MessagingAsyncResult BeginPing(Guid[] targets,
            AsyncCallback callback, object state)
        {
            return BeginSend<object>(targets, null, WellKnownTags.Ping,
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
        /// Receives a message.
        /// </summary>
        public ICastable Receive(out Guid from, out int? tag)
        {
            while (true)
            {
                TrackableDto dto = receiveQueue.Dequeue();
                tag = dto.Tag;
                from = dto.FromId;
                return dto;
            }
        }

        /// <summary>
        /// Receives a message.
        /// </summary>
        public TResult Receive<TResult>(out Guid from, out int? tag)
        {
            return Receive(out from, out tag).Cast<TResult>();
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the remote endpoint and the endpoint resolved by ID
        /// are not the same.
        /// </summary>
        public event EventHandler<EndPointMismatchedEventArgs<TEndPoint>> EndPointMismatched;

        private void OnEndPointMismatched(Guid id, TEndPoint resolvedEndPoint)
        {
            EventHandler<EndPointMismatchedEventArgs<TEndPoint>> handler =
                EndPointMismatched;
            if (handler != null)
            {
                handler(this, new EndPointMismatchedEventArgs<TEndPoint>(
                    id, resolvedEndPoint));
            }
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
                communicator.Close();
                receiveQueue.Dispose();
            }
        }
    }
}
