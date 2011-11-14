using System;
using System.Net;
using Cloudy.Examples.Chat.Shared;
using NLog;

namespace Cloudy.Examples.Chat.Client
{
    public static class Program
    {
        private static readonly Logger Logger =
            LogManager.GetCurrentClassLogger();

        private static readonly int ChatServerPortNumber =
            Configuration.GetInt32("ChatServerPortNumber");

        public static void Main(string[] args)
        {
            Logger.Info("Starting the client ...");
            Client client = new Client();
            if (!client.ConnectToMainServer(new IPEndPoint(
                IPAddress.Loopback, ChatServerPortNumber)))
            {
                Logger.Error("Connecting to the server has failed.");
                client.Close();
                return;
            }

            while (Console.ReadLine() != null)
            {
            }

            client.Close();
        }
    }
}
