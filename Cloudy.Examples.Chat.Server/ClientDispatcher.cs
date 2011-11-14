using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading;
using Cloudy.Examples.Chat.Shared;
using Cloudy.Examples.Chat.Shared.Values;
using Cloudy.Helpers;
using Cloudy.Messaging;
using Cloudy.Messaging.Enums;
using Cloudy.Messaging.Exceptions;
using Cloudy.Messaging.Interfaces;
using Cloudy.Messaging.Structures;
using Cloudy.Networking.Values;
using NLog;

namespace Cloudy.Examples.Chat.Server
{
    internal class ClientDispatcher
    {
        private static readonly Logger Logger =
            LogManager.GetCurrentClassLogger();

        private readonly int chatServerPortNumber =
            Configuration.GetInt32("ChatServerPortNumber");

        private readonly Guid myId = Guid.NewGuid();

        private readonly Dictionary<Guid, IPEndPoint> externalEndPointCache =
            new Dictionary<Guid, IPEndPoint>();

        private readonly Dictionary<Guid, IPEndPoint> localEndPointCache =
            new Dictionary<Guid, IPEndPoint>();

        private readonly Dictionary<Guid, string> nicknameCache =
            new Dictionary<Guid, string>();

        private readonly MessageDispatcher<IPEndPoint> dispatcher;

        private Thread serveClientsThread;

        private Thread dispatcherThread;

        public ClientDispatcher()
        {
            Communicator<IPEndPoint> communicator = new Communicator<IPEndPoint>(
                new UdpClientRawCommunicator(new UdpClient(chatServerPortNumber)));
            this.dispatcher = new MessageDispatcher<IPEndPoint>(
                myId, ResolveEndPoint, communicator);
        }

        private bool ResolveEndPoint(Guid id, out IPEndPoint endPoint)
        {
            return externalEndPointCache.TryGetValue(id, out endPoint);
        }

        public void Start()
        {
            dispatcherThread = new Thread(RunDispatcher);
            dispatcherThread.IsBackground = true;
            dispatcherThread.Start();

            serveClientsThread = new Thread(ServeClientMessages);
            serveClientsThread.IsBackground = true;
            serveClientsThread.Start();
        }

        private void ServeClientMessages()
        {
            Logger.Info("Serving clients on port {0} ...", chatServerPortNumber);
            try
            {
                while (true)
                {
                    try
                    {
                        ProcessOneClientMessage();
                    }
                    catch (SerializationException)
                    {
                        continue;
                    }
                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
            finally
            {
                Logger.Info("Serving of clients was terminated.");
            }
        }

        private void ProcessOneClientMessage()
        {
            int? tag;
            Guid fromId;
            Logger.Info("Awaiting for a message ...");
            ICastable message = dispatcher.Receive(out fromId, out tag);
            Logger.Info("Received message with tag: {0}", tag);
            switch (tag)
            {
                case WellKnownTags.Join:
                    JoinValue value = message.Cast<JoinValue>();
                    Logger.Info("Join request from {0}", value.Id);
                    externalEndPointCache[value.Id] = value.ExternalEndPoint;
                    localEndPointCache[value.Id] = value.SourceEndPoint;
                    string nickname;
                    if (!nicknameCache.TryGetValue(value.Id, out nickname))
                    {
                        nickname = nicknameCache[value.Id] = String.Format(
                            "Anonymous-{0}", value.Id.ToString().Substring(0, 8));
                    }
                    JoinedValue response = new JoinedValue { MyId = myId, Nickname = nickname };
                    dispatcher.BeginSend(value.Id, response, WellKnownTags.Join, null, null);
                    break;
                default:
                    Logger.Warn("Ignored tag: {0} received from {1}", tag, fromId);
                    break;
            }
        }

        private void RunDispatcher()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        dispatcher.ProcessIncomingMessages(1);
                    }
                    catch (SerializationException)
                    {
                        continue;
                    }
                }
            }
            catch (ThreadAbortException)
            {
                Logger.Info("Dispatcher is aborted.");
                return;
            }
        }

        public void Close()
        {
            serveClientsThread.Abort();
            dispatcherThread.Abort();
            dispatcher.Dispose();
        }
    }
}
