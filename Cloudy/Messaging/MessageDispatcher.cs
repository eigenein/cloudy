using System;

namespace Cloudy.Messaging
{
    /// <summary>
    /// Sends, receives and tracks messages.
    /// </summary>
    public class MessageDispatcher
    {
        #region Private Fields

        private readonly MessageStream messageStream;

        #endregion

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="messageStream">The underlying message stream.</param>
        public MessageDispatcher(MessageStream messageStream)
        {
            this.messageStream = messageStream;
        }

        #region ID creating

        private ulong id;

        private readonly object idLock = new object();

        /// <summary>
        /// Creates the new unique ID.
        /// </summary>
        protected ulong CreateId()
        {
            lock (idLock)
            {
                return id++;
            }
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

        #endregion

        #region Public Methods

        /// <summary>
        /// Processes incoming messages.
        /// </summary>
        /// <param name="count">The count of messages to be processed.</param>
        public void ProcessMessages(int count)
        {
            // TODO
        }

        #endregion
    }
}
