using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using Cloudy.Examples.Chat.Shared;
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

        public void ServeClients()
        {
            Logger.Info("Serving clients ...");
            try
            {
                while (true)
                {
                    try
                    {
                        ProcessOneMessage();
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
        }

        private void ProcessOneMessage()
        {
            int? tag;
            Guid fromId;
            // TODO: Process a Join request. At least...
        }
    }
}
