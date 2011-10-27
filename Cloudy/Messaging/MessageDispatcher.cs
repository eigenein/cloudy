using System;

namespace Cloudy.Messaging
{
    public class MessageDispatcher
    {
        #region Private Fields

        private readonly MessageStream messageStream;

        #endregion

        public MessageDispatcher(MessageStream messageStream)
        {
            this.messageStream = messageStream;
        }

        #region Tagging

        private ulong tag;

        private readonly object taggingLock = new object();

        protected ulong CreateTag()
        {
            lock (taggingLock)
            {
                return tag++;
            }
        }

        #endregion

        #region Properties

        public MessageStream MessageStream
        {
            get { return messageStream; }
        }

        #endregion

        #region Public Methods

        

        #endregion
    }
}
