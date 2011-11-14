using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Cloudy.Examples.Chat.Shared;
using Cloudy.Examples.Chat.Shared.Values;
using Cloudy.Helpers;
using Cloudy.Messaging;
using Cloudy.Messaging.Enums;
using Cloudy.Messaging.Interfaces;
using Cloudy.Networking.Values;
using NLog;

namespace Cloudy.Examples.Chat.Client
{
    internal class Client
    {
        private static readonly Logger Logger =
            LogManager.GetCurrentClassLogger();

        private readonly Guid myId = Guid.NewGuid();

        private readonly int sourcePortNumber;

        private readonly MessageDispatcher<IPEndPoint> dispatcher;

        private readonly Dictionary<Guid, IPEndPoint> endPointCache =
            new Dictionary<Guid, IPEndPoint>();

        private Guid serverId;

        private string nickname;

        public Client(int sourcePortNumber)
        {
            this.sourcePortNumber = sourcePortNumber;
            UdpClient client = new UdpClient(sourcePortNumber);
            Communicator<IPEndPoint> communicator = new Communicator<IPEndPoint>(
                new UdpClientRawCommunicator(client));
            this.dispatcher = new MessageDispatcher<IPEndPoint>(
                myId, ResolveEndPoint, communicator);
        }

        public string Nickname
        {
            get { return nickname; }
        }

        public bool ConnectToMainServer(IPEndPoint endPoint)
        {
            Logger.Info("Connecting to {0} ...", endPoint);
            // TODO: Pass both local & external EndPoints
            // TODO: Determine external endpoint here instead of Program.Main?
            dispatcher.Communicator.SendTagged(WellKnownTags.Join,
                new JoinValue { Id = myId }, endPoint);
            int? tag = null;
            Guid fromId;
            ICastable message = null;
            if (!InvokeHelper.CallWithTimeout(
                () => message = dispatcher.Receive(out fromId, out tag),
                new TimeSpan(0, 0, 10)))
            {
                Logger.Error("Not joined: timeout.");
                return false;
            }
            if (tag != WellKnownTags.Join)
            {
                Logger.Error("Not joined: received tag: {0}.", tag);
                return false;
            }
            JoinedValue joinedValue = message.Cast<JoinedValue>();
            endPointCache[serverId = joinedValue.MyId] = endPoint;
            nickname = joinedValue.Nickname;
            Logger.Info("Joined. Server ID: {0}, cached nickname: {1}",
                serverId, nickname);
            return true;
        }

        private bool ResolveEndPoint(Guid id, out IPEndPoint endPoint)
        {
            return endPointCache.TryGetValue(id, out endPoint);
        }
    }
}
