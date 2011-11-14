using System;
using System.Net;
using Cloudy.Examples.Chat.Shared;
using Cloudy.Networking.IP;
using NLog;

namespace Cloudy.Examples.Chat.Client
{
    public static class Program
    {
        private static readonly Random Random = new Random();

        private static readonly int MySourcePortNumber =
            Configuration.GetInt32("BaseSourcePortNumber") + Random.Next(100);

        private static readonly int ExternalIPEndPointServerPortNumber =
            Configuration.GetInt32("ExternalIPEndPointServerPortNumber");

        private static readonly Logger Logger =
            LogManager.GetCurrentClassLogger();

        private static IPEndPoint myExternalEndPoint;

        public static void Main(string[] args)
        {
            Logger.Info("Starting the client ...");

            if (!RequestExternalEndPoint())
            {
                return;
            }

            string line;
            while ((line = Console.ReadLine()) != line)
            {
            }
        }

        private static bool RequestExternalEndPoint()
        {
            Logger.Info("Requesting for external IP endpoint ...");
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Loopback,
                ExternalIPEndPointServerPortNumber);
            if (!ExternalIPEndPointClient.RequestInformation(
                serverEndPoint, MySourcePortNumber, out myExternalEndPoint))
            {
                Logger.Error("Couldn't obtain external IP endpoint.");
                return false;
            }
            Logger.Info("Got external IP endpoint: {0}", myExternalEndPoint);
            return true;
        }
    }
}
