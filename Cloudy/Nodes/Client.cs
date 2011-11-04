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
            MessageStream inputStream = new MessageStream(controlStream);
            this.ControlMessageDispatcher = new MessageDispatcher(id,
                ResolveMasterStream, inputStream);
            this.id = id;
        }

        private bool ResolveMasterStream(Guid idToResolve, out MessageStream stream)
        {
            stream = ControlMessageDispatcher.InputStream;
            return true;
        }

        /// <summary>
        /// Gets the unique identifier of the node.
        /// </summary>
        public Guid Id
        {
            get { return id; }
        }

        /// <summary>
        /// Performs an authetication.
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns>Whether authentication was successful.</returns>
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
