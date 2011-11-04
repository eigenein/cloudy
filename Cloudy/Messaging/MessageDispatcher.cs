using System;
using System.Collections.Generic;
using System.Threading;
using Cloudy.Helpers;
using Cloudy.Messaging.Enums;
using Cloudy.Messaging.Interfaces;
using Cloudy.Messaging.Structures;

namespace Cloudy.Messaging
{
    /// <summary>
    /// Sends, receives and tracks messages.
    /// </summary>
    public class MessageDispatcher : IDisposable
    {
        #region Private Fields

        private readonly MessageStream inputStream;

        private readonly Guid fromId;

        private readonly Func<Guid, MessageStream> resolveStream;

        private readonly Dictionary<long, MessagingAsyncResult> sendQueue =
            new Dictionary<long, MessagingAsyncResult>();

        private readonly BlockingQueue<Dto> receiveQueue = new BlockingQueue<Dto>();

        #endregion

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="resolveStream">Used to find a stream by a recipient identifier.</param>
        /// <param name="inputStream">The underlying message stream.</param>
        /// <param name="fromId">The sender identifier.</param>
        public MessageDispatcher(Guid fromId, Func<Guid, MessageStream> resolveStream,
            MessageStream inputStream)
        {
            if (!inputStream.CanRead)
            {
                throw new InvalidOperationException("The stream is not readable.");
            }
            this.inputStream = inputStream;
            this.resolveStream = resolveStream;
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
        public MessageStream InputStream
        {
            get { return inputStream; }
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
            Dto message;
            int processedMessagesCount = 0;
            // Loop through messages.
            while (processedMessagesCount < count && 
                (message = inputStream.Read<Dto>()) != null)
            {
                if (message.Tag == WellKnownTags.DeliveryNotification)
                {
                    MessagingAsyncResult ar = sendQueue[message.TrackingId];
                    ar.Notify();
                    if (ar.IsCompleted)
                    {
                        sendQueue.Remove(message.TrackingId);
                        ar.Dispose();
                    }
                }
                else
                {
                    // Send the delivery notification.
                    resolveStream(message.FromId).Write(
                        new Dto(fromId, message.TrackingId, 
                        WellKnownTags.DeliveryNotification, null));
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
        public MessagingAsyncResult BeginSend<T>(Guid[] recipients, 
            T message, int? tag, AsyncCallback callback, object state)
        {
            Dto dto = new Dto<T>(fromId, CreateTrackingId(), tag, message).AsUntyped();
            foreach (Guid recipient in recipients)
            {
                resolveStream(recipient).Write(dto);
            }
            MessagingAsyncResult ar = new MessagingAsyncResult(recipients.Length,
                callback, state);
            sendQueue.Add(dto.TrackingId, ar);
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
        public ICastableValueProvider Receive(out Guid from, out int? tag)
        {
            while (true)
            {
                Dto dto = receiveQueue.Dequeue();
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
            return Receive(out from, out tag).GetValue<TResult>();
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
                inputStream.Dispose();
                receiveQueue.Dispose();
            }
        }
    }
}
