using System;
using Cloudy.Examples.Chat.Shared;
using Cloudy.Networking.Events;
using Cloudy.Networking.IP;
using NLog;

namespace Cloudy.Examples.Chat.Server
{
    public static class Program
    {
        private static readonly Logger Logger =
            LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            Logger.Info("Starting the server ...");

            Logger.Info("Starting the external IP endpoint server ...");
            ExternalIPEndPointServer endPointServer = new ExternalIPEndPointServer(
                Constants.ExternalIPEndPointServerPortNumber);
            endPointServer.ExternalIPEndPointRequested += OnExternalIPEndPointRequested;
            endPointServer.Start();

            Logger.Info("Press any key to terminate.");
            Console.ReadKey();

            endPointServer.Stop();
            endPointServer.Dispose();
        }

        private static void OnExternalIPEndPointRequested(object sender, 
            ExternalIPEndPointRequestedEventArgs e)
        {
            Logger.Info("External IP is requested by {0}", e.EndPoint);
        }
    }
}
