using System;
using System.Net.Sockets;
using Cloudy.Messaging;

namespace Cloudy.Connections.Abstract
{
    public abstract class TcpClientConnection : IDisposable
    {
        #region Private Fields

        private readonly MessageDispatcher tcpMessageDispatcher;

        #endregion

        #region Constructors

        protected TcpClientConnection(TcpClient client)
        {
            this.tcpMessageDispatcher = new MessageDispatcher(
                new MessageStream(client.GetStream()));
        }

        #endregion

        #region Public Methods

        public bool Authenticate(Guid clientId, byte[] authenticationData)
        {
            throw new NotImplementedException();
        }

        public virtual void Close()
        {
            tcpMessageDispatcher.MessageStream.Close();
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Close();
        }

        #endregion
    }
}
