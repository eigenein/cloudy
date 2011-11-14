using System;
using System.Threading;
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

        private static readonly int ExternalIPEndPointServerPortNumber =
            Configuration.GetInt32("ExternalIPEndPointServerPortNumber");

        public static void Main(string[] args)
        {
            Logger.Info("Starting the server ...");
            ExternalIPEndPointServer endPointServer = StartExternalIPEndPointServer();
            ClientDispatcher clientDispatcher = new ClientDispatcher();
            clientDispatcher.Start();

            Logger.Info("Press any key to terminate.");
            Console.ReadKey();

            clientDispatcher.Close();
            endPointServer.Stop();
            endPointServer.Dispose();
        }

        private static ExternalIPEndPointServer StartExternalIPEndPointServer()
        {
            Logger.Info("Starting the external IP endpoint server ...");
            ExternalIPEndPointServer endPointServer = new ExternalIPEndPointServer(
                ExternalIPEndPointServerPortNumber);
            endPointServer.ExternalIPEndPointRequested += OnExternalIPEndPointRequested;
            endPointServer.Start();
            return endPointServer;
        }

        private static void OnExternalIPEndPointRequested(object sender, 
            ExternalIPEndPointRequestedEventArgs e)
        {
            Logger.Info("External IP is requested by {0}", e.EndPoint);
        }
    }
}
