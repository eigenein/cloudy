using System;
using System.Net;
using Cloudy.Examples.Chat.Shared;
using Cloudy.Networking.IP;
using NLog;

namespace Cloudy.Examples.Chat.Client
{
    public static class Program
    {
        private static readonly Logger Logger =
            LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            Logger.Info("Starting the client ...");

            Logger.Info("Requesting for external IP endpoint ...");
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Loopback,
                Constants.ExternalIPEndPointServerPortNumber);
            IPEndPoint externalEndPoint;
            if (!ExternalIPEndPointClient.RequestInformation(
                serverEndPoint, Constants.PrimaryPortNumber, out externalEndPoint))
            {
                Logger.Error("Couldn't obtain external IP endpoint.");
                return;
            }
            Logger.Info("Got external IP endpoint: {0}", externalEndPoint);

            string line;
            while ((line = Console.ReadLine()) != line)
            {
            }
        }
    }
}
