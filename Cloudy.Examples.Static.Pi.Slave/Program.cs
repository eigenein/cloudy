using System;
using System.Net;
using System.Threading;
using Cloudy.Examples.Shared.Configuration;
using Cloudy.Examples.Shared.Helpers;
using NLog;

namespace Cloudy.Examples.Static.Pi.Slave
{
    static class Program
    {
        private static readonly Logger Logger =
            LogManager.GetCurrentClassLogger();

        private static readonly int MasterPort = ApplicationSettings.GetInteger(
            "MasterPort");

        private static readonly IPAddress MasterAddress = ApplicationSettings.GetIPAddress(
            "MasterAddress");

        private static readonly IPAddress LocalAddress = ApplicationSettings.GetIPAddress(
            "LocalAddress");

        private static readonly int LocalPort = 
            ApplicationSettings.GetInteger("BaseLocalPort") + RandomExtensions.Instance.Next(100);

        static void Main(string[] args)
        {
            Logger.Info("Starting Slave ...");
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            ExampleSlave slave = new ExampleSlave(new IPEndPoint(
                LocalAddress, LocalPort));
            slave.Joined += (sender, e) => Logger.Info("Joined as {0}", e.ExternalEndPoint);
            ThreadPool.QueueUserWorkItem(RunSlave, slave);

            Logger.Info("Joining the network ...");
            slave.JoinNetwork(new IPEndPoint(MasterAddress, MasterPort), null);

            Logger.Info("Press Return to quit.");
            Console.ReadLine();
            Logger.Info("Quit");
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Error(e.ExceptionObject.ToString());
        }

        private static void RunSlave(object state)
        {
            Computing.Slave slave = (Computing.Slave)state;
            while (slave.State != Computing.Enums.SlaveState.Left)
            {
                slave.ProcessIncomingMessages(1);
            }
        }
    }
}
