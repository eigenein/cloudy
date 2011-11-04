using System;
using System.IO;
using Cloudy.Messaging;

namespace Cloudy.Nodes
{
    /// <summary>
    /// Represents an abstract client.
    /// </summary>
    public abstract class Client : IDisposable
    {
        private readonly Guid id;

        /// <summary>
        /// Contains only a master fake ID.
        /// </summary>
        private readonly Guid[] defaultRecipients = new Guid[] { Guid.Empty };

        protected readonly MessageDispatcher ControlMessageDispatcher;

        protected Client(Stream controlStream, Guid id)
        {
            MessageStream inputStream = new MessageStream(controlStream);
            this.ControlMessageDispatcher = new MessageDispatcher(id,
                recipientId => inputStream, inputStream);
            this.id = id;
        }

        /// <summary>
        /// Gets the unique identifier of the node.
        /// </summary>
        public Guid Id
        {
            get { return id; }
        }

        public bool Authenticate<T>(T credentials)
        {
            throw new NotImplementedException();
        }

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
                ControlMessageDispatcher.Dispose();
            }
        }
    }
}
