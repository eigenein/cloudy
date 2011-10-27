using System;
using System.Net.Sockets;
using Cloudy.Helpers;
using Cloudy.Messaging;

namespace Cloudy.Connections.Abstract
{
    public abstract class TcpUdpClientConnection : TcpClientConnection
    {
        #region Private Fields

        private readonly MessageDispatcher udpMessageDispatcher;

        #endregion

        protected TcpUdpClientConnection(TcpClient tcpClient, UdpClient udpClient)
            : base(tcpClient)
        {
            udpMessageDispatcher = new MessageDispatcher(
                new MessageStream(new UdpStream(udpClient)));
        }

        #region Public Methods

        public override void Close()
        {
            udpMessageDispatcher.MessageStream.Close();
            base.Close();
        }

        #endregion
    }
}
