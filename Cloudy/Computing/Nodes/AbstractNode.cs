using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using Cloudy.Collections;
using Cloudy.Helpers;
using Cloudy.Messaging;
using Cloudy.Messaging.Interfaces;
using Cloudy.Messaging.Raw;

namespace Cloudy.Computing.Nodes
{
    public abstract class AbstractNode : IDisposable
    {
        private readonly Dictionary<int, Action<IPEndPoint, IMessage>> handlers =
            new Dictionary<int, Action<IPEndPoint, IMessage>>();

        private readonly BlockingMultiDictionary<IPEndPoint, IMessage> unhandledMessages =
            new BlockingMultiDictionary<IPEndPoint, IMessage>();

        private readonly UdpClientSimpleCommunicator udpClientSimpleCommunicator;

        private readonly MessageDispatcher<IPEndPoint> dispatcher;

        protected readonly int Port;

        protected AbstractNode(int port)
        {
            this.SendTimeout = new TimeSpan(0, 0, 3);
            this.ReceiveTimeout = new TimeSpan(0, 0, 3);
            this.Port = port;
            UdpClient client = new UdpClient(port);
            client.DontFragment = true;
            this.udpClientSimpleCommunicator = new UdpClientSimpleCommunicator(client);
            this.dispatcher = new MessageDispatcher<IPEndPoint>(
                new Communicator<IPEndPoint>(udpClientSimpleCommunicator));
            this.udpClientSimpleCommunicator.BytesSent += OnBytesSent;
            this.udpClientSimpleCommunicator.BytesReceived += OnBytesReceived;
        }

        public TimeSpan ReceiveTimeout { get; set; }

        public TimeSpan SendTimeout { get; set; }

        #region Events

        public event EventHandler MessageHandled;

        public event ParameterizedEventHandler<byte[]> BytesSent;

        public event ParameterizedEventHandler<byte[]> BytesReceived;

        private void OnBytesReceived(object sender, EventArgs<byte[]> e)
        {
            if (BytesReceived != null)
            {
                BytesReceived(this, e);
            }
        }

        private void OnBytesSent(object sender, EventArgs<byte[]> e)
        {
            if (BytesSent != null)
            {
                BytesSent(this, e);
            }
        }

        #endregion

        public void AddHandler(int tag, Action<IPEndPoint, IMessage> handler)
        {
            handlers[tag] = handler;
        }

        public void ProcessIncomingMessages(int count)
        {
            dispatcher.ProcessIncomingMessages(count);
        }

        public void HandleMessages(int count)
        {
            while (count != 0)
            {
                IPEndPoint remoteEndPoint;
                int tag;
                IMessage message = dispatcher.Receive(out remoteEndPoint, out tag);
                Action<IPEndPoint, IMessage> handler;
                if (handlers.TryGetValue(tag, out handler))
                {
                    handler(remoteEndPoint, message);
                }
                else
                {
                    unhandledMessages.Enqueue(remoteEndPoint, message);
                }
                count -= 1;
                if (MessageHandled != null)
                {
                    MessageHandled(this, new EventArgs());
                }
            }
        }

        protected void Ping(IPEndPoint endPoint)
        {
            dispatcher.Ping(endPoint, SendTimeout);
        }

        #region Send

        /// <summary>
        /// Sends the message asynchronously, but without tracking.
        /// </summary>
        protected void SendAsync<T>(IPEndPoint endPoint, T message, int tag)
        {
            dispatcher.SendAsync(endPoint, message, tag);
        }

        protected virtual void Send<T>(IPEndPoint endPoint, T message, int tag)
        {
            dispatcher.Send(endPoint, message, tag, SendTimeout);
        }

        #endregion

        #region ReceiveFrom

        protected IMessage ReceiveFrom(IPEndPoint endPoint)
        {
            return unhandledMessages.Dequeue(endPoint, ReceiveTimeout);
        }

        protected T ReceiveFrom<T>(IPEndPoint endPoint)
        {
            return unhandledMessages.Dequeue(endPoint, ReceiveTimeout).Get<T>();
        }

        protected T ReceiveFrom<T>(IPEndPoint endPoint, TimeSpan timeout)
        {
            return unhandledMessages.Dequeue(endPoint, timeout).Get<T>();
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        [SuppressMessage("Microsoft.Usage", "CA2213",
            Justification = "Simple communicator is disposed inside the wrapping Communicator.")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                unhandledMessages.Dispose();
                dispatcher.Dispose();
            }
        }
    }
}
