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
using Cloudy.Messaging.Interfaces;
using Cloudy.Networking.IP;
using Cloudy.Networking.Values;
using NLog;

namespace Cloudy.Examples.Chat.Client
{
    internal class Client
    {
        private static readonly Logger Logger =
            LogManager.GetCurrentClassLogger();

        private static readonly int ExternalIPEndPointServerPortNumber =
            Configuration.GetInt32("ExternalIPEndPointServerPortNumber");

        private static readonly Random Random = new Random();

        private readonly Guid myId = Guid.NewGuid();

        private readonly int mySourcePortNumber =
            Configuration.GetInt32("BaseSourcePortNumber") + Random.Next(100);

        private IPEndPoint myExternalEndPoint;

        private readonly Dictionary<Guid, IPEndPoint> endPointCache =
            new Dictionary<Guid, IPEndPoint>();

        private Thread processingThread;

        private MessageDispatcher<IPEndPoint> dispatcher;

        private Guid serverId;

        private string nickname;

        public string Nickname
        {
            get { return nickname; }
        }

        public bool ConnectToMainServer(IPEndPoint serverEndPoint)
        {
            Logger.Info("Connecting to {0} ...", serverEndPoint);
            if (!RequestExternalEndPoint())
            {
                return false;
            }
            Communicator<IPEndPoint> communicator = new Communicator<IPEndPoint>(
                new UdpClientRawCommunicator(new UdpClient(mySourcePortNumber)));
            dispatcher = new MessageDispatcher<IPEndPoint>(myId, ResolveEndPoint, communicator);
            processingThread = new Thread(RunDispatcher);
            processingThread.IsBackground = true;
            processingThread.Start();
            dispatcher.BeginSend(serverEndPoint,
                new JoinValue(myId, GetLocalEndPoint(), myExternalEndPoint),
                WellKnownTags.Join, null, null);
            Logger.Info("Awaiting for joining response ...");
            int? tag = null;
            Guid fromId;
            ICastable message = null;
            if (!InvokeHelper.CallWithTimeout(
                () => message = dispatcher.Receive(out fromId, out tag),
                new TimeSpan(0, 0, 10)))
            {
                Logger.Error("Not joined: receive timeout.");
                return false;
            }
            if (tag != WellKnownTags.Join)
            {
                Logger.Error("Not joined: received tag: {0}.", tag);
                return false;
            }
            JoinedValue joinedValue = message.Cast<JoinedValue>();
            endPointCache[serverId = joinedValue.MyId] = serverEndPoint;
            nickname = joinedValue.Nickname;
            Logger.Info("Joined. Server ID: {0}, cached nickname: {1}",
                serverId, nickname);
            return true;
        }

        public void Close()
        {
            processingThread.Abort();
            dispatcher.Dispose();
        }

        private bool ResolveEndPoint(Guid id, out IPEndPoint endPoint)
        {
            return endPointCache.TryGetValue(id, out endPoint);
        }

        private void RunDispatcher(object state)
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

        private bool RequestExternalEndPoint()
        {
            Logger.Info("Requesting for external IP endpoint ...");
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Loopback,
                ExternalIPEndPointServerPortNumber);
            if (!ExternalIPEndPointClient.RequestInformation(
                serverEndPoint, mySourcePortNumber, out myExternalEndPoint))
            {
                Logger.Error("Couldn't obtain external IP endpoint.");
                return false;
            }
            Logger.Info("Got external IP endpoint: {0}", myExternalEndPoint);
            return true;
        }

        private IPEndPoint GetLocalEndPoint()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ipAddress in host.AddressList)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    return new IPEndPoint(ipAddress, mySourcePortNumber);
                }
            }
            return null;
        }
    }
}
