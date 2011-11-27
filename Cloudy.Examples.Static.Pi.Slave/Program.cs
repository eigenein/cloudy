using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cloudy.Computing;
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

        private static readonly int BaseLocalPort = 
            ApplicationSettings.GetInteger("BaseLocalPort");

        private static readonly int Port = BaseLocalPort +
            RandomExtensions.Instance.Next(1, 1000);

        private static readonly IPAddress LocalAddress =
            ApplicationSettings.GetIPAddress("LocalAddress");

        private static readonly IPEndPoint MasterEndPoint = new IPEndPoint(
            ApplicationSettings.GetIPAddress("MasterAddress"),
            ApplicationSettings.GetInteger("MasterPort"));

        private static readonly int SlotsCount =
            ApplicationSettings.GetInteger("SlotsCount");

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            Logger.Info("Starting Slave ...");
            SlaveNode slave = new SlaveNode(Port, LocalAddress, SlotsCount);
            ThreadPool.QueueUserWorkItem(HandleMessages, slave);
            ThreadPool.QueueUserWorkItem(ProcessIncomingMessages, slave);

            Logger.Info("Joining the network at {0} ...", MasterEndPoint);
            if (!InvokeHelper.RepeatedCall(() => slave.Join(MasterEndPoint), 3))
            {
                Logger.Error("Join failed.");
            }

            Logger.Info("Press Enter to quit.");
            Console.ReadLine();
            slave.Dispose();
        }

        private static void HandleMessages(object state)
        {
            AbstractSlaveNode slave = (AbstractSlaveNode)state;
            while (slave.State != Computing.Enums.SlaveState.Left)
            {
                slave.HandleMessages(1);
            }
        }

        private static void ProcessIncomingMessages(object state)
        {
            AbstractSlaveNode slave = (AbstractSlaveNode)state;
            while (slave.State != Computing.Enums.SlaveState.Left)
            {
                try
                {
                    slave.ProcessIncomingMessages(1);
                }
                catch (SocketException ex)
                {
                    Logger.Warn(ex.Message);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex.ToString());
                }
            }
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Error(e.ExceptionObject.ToString());
        }
    }
}
