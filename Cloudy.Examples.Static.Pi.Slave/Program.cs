using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cloudy.Examples.Shared.Configuration;
using Cloudy.Examples.Shared.Helpers;
using Cloudy.Helpers;
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

        private static readonly IPEndPoint MasterEndPoint = new IPEndPoint(
            MasterAddress, MasterPort);

        private static readonly IPAddress LocalAddress = ApplicationSettings.GetIPAddress(
            "LocalAddress");

        private static readonly int LocalPort = 
            ApplicationSettings.GetInteger("BaseLocalPort") + RandomExtensions.Instance.Next(100);

        private static readonly int SlotsCount = ApplicationSettings.GetInteger("SlotsCount");

        static void Main(string[] args)
        {
            Logger.Info("Starting Slave ...");
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            ExampleSlave slave = new ExampleSlave(new IPEndPoint(
                LocalAddress, LocalPort), SlotsCount);
            ThreadPool.QueueUserWorkItem(RunSlave, slave);

            Logger.Info("Joining the network ...");
            if (!InvokeHelper.RepeatedCall(() =>
                slave.JoinNetwork(MasterEndPoint, null), 3))
            {
                slave.Dispose();
                Logger.Error("Couldn't connect to the master at {0}", MasterEndPoint);
                return;
            }

            Logger.Info("Press Return to quit.");
            Console.ReadLine();
            Logger.Info("Quit");
            slave.Dispose();
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
                try
                {
                    slave.ProcessIncomingMessages(1);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex.Message);
                    continue;
                }
            }
        }
    }
}
