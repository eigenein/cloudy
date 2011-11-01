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

        protected readonly MessageDispatcher ControlMessageDispatcher;

        protected Client(Stream controlStream, Guid id)
        {
            this.ControlMessageDispatcher = new MessageDispatcher(
                new MessageStream(controlStream));
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
            ControlMessageDispatcher.MessageStream.Close();
        }

        #endregion
    }
}
