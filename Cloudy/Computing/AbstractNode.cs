using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Cloudy.Collections;
using Cloudy.Messaging;
using Cloudy.Messaging.Interfaces;
using Cloudy.Messaging.Raw;

namespace Cloudy.Computing
{
    public abstract class AbstractNode : IDisposable
    {
        private readonly Dictionary<int, Func<IPEndPoint, IMessage, bool>> handlers =
            new Dictionary<int, Func<IPEndPoint, IMessage, bool>>();

        private readonly BlockingMultiDictionary<IPEndPoint, IMessage> unhandledMessages =
            new BlockingMultiDictionary<IPEndPoint, IMessage>();

        private readonly MessageDispatcher<IPEndPoint> dispatcher;

        protected readonly int Port;

        protected AbstractNode(int port)
        {
            this.SendTimeout = new TimeSpan(0, 0, 1);
            this.ReceiveTimeout = new TimeSpan(0, 0, 3);
            this.Port = port;
            UdpClient client = new UdpClient(port);
            client.DontFragment = true;
            this.dispatcher = new MessageDispatcher<IPEndPoint>(
                new Communicator<IPEndPoint>(new UdpClientSimpleCommunicator(client)));
        }

        public TimeSpan ReceiveTimeout { get; set; }

        public TimeSpan SendTimeout { get; set; }

        public event EventHandler MessageHandled;

        public void AddHandler(int tag, Func<IPEndPoint, IMessage, bool> handler)
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
                Func<IPEndPoint, IMessage, bool> handler;
                if (!handlers.TryGetValue(tag, out handler) ||
                    !handler(remoteEndPoint, message))
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

        protected void SendAsync<T>(IPEndPoint endPoint, T message, int tag)
        {
            dispatcher.SendAsync(endPoint, message, tag);
        }

        protected virtual void Send<T>(IPEndPoint endPoint, T message, int tag)
        {
            dispatcher.Send(endPoint, message, tag, SendTimeout);
        }

        protected IMessage ReceiveFrom(IPEndPoint endPoint)
        {
            return unhandledMessages.Dequeue(endPoint, ReceiveTimeout);
        }

        protected T ReceiveFrom<T>(IPEndPoint endPoint)
        {
            return unhandledMessages.Dequeue(endPoint, ReceiveTimeout).Get<T>();
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

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
