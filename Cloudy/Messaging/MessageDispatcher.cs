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

        private readonly MessageStream messageStream;

        private readonly Dictionary<long, MessagingAsyncResult> sendQueue =
            new Dictionary<long, MessagingAsyncResult>();

        private readonly BlockingQueue<Dto> receiveQueue = new BlockingQueue<Dto>();

        #endregion

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="messageStream">The underlying message stream.</param>
        public MessageDispatcher(MessageStream messageStream)
        {
            this.messageStream = messageStream;
            if (!messageStream.CanRead)
            {
                throw new InvalidOperationException("The stream is not readable.");
            }
            if (!messageStream.CanWrite)
            {
                throw new InvalidOperationException("The stream is not writeable.");
            }
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
        /// Gets the underlying message stream.
        /// </summary>
        public MessageStream MessageStream
        {
            get { return messageStream; }
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
        public int ProcessMessages(int count)
        {
            Dto message;
            int processedMessagesCount = 0;
            // Loop through messages.
            while (processedMessagesCount < count && 
                (message = messageStream.Read<Dto>()) != null)
            {
                if (message.Tag == WellKnownTags.DeliveryNotification)
                {
                    sendQueue[message.TrackingId].SetCompleted();
                    sendQueue.Remove(message.TrackingId);
                }
                else
                {
                    // Send the delivery notification.
                    messageStream.Write(new Dto(message.TrackingId, 
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
        public MessagingAsyncResult BeginSend<T>(T message, int? tag,
            AsyncCallback callback, object state)
        {
            long trackingId = CreateTrackingId();
            messageStream.Write(new Dto<T>(trackingId, tag, message));
            MessagingAsyncResult ar = new MessagingAsyncResult(callback, state);
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
        public MessagingAsyncResult BeginPing(AsyncCallback callback, object state)
        {
            long trackingId = CreateTrackingId();
            messageStream.Write(new Dto(trackingId, WellKnownTags.Ping, null));
            MessagingAsyncResult ar = new MessagingAsyncResult(callback, state);
            sendQueue.Add(trackingId, ar);
            return ar;
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
        public ICastableValueProvider Receive(out int? tag)
        {
            while (true)
            {
                Dto dto = receiveQueue.Dequeue();
                tag = dto.Tag;
                return dto;
            }
        }

        /// <summary>
        /// Receives a message.
        /// </summary>
        public TResult Receive<TResult>(out int? tag)
        {
            return Receive(out tag).GetValue<TResult>();
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
                messageStream.Dispose();
                receiveQueue.Dispose();
            }
        }
    }
}
