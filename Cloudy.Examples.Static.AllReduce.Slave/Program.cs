using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cloudy.Examples.Shared.Configuration;
using Cloudy.Examples.Static.AllReduce.Slave;
using NLog;

namespace Cloudy.Examples.Static.AllReduce.Slave
{
    static class Program
    {
        private static readonly Logger Logger =
            LogManager.GetCurrentClassLogger();

        private static readonly int LocalPort =
            ApplicationSettings.GetInteger("LocalPort");

        private static readonly IPAddress LocalAddress =
            ApplicationSettings.GetIPAddress("LocalAddress");

        private static readonly string MasterAddress =
            ApplicationSettings.GetString("MasterAddress");

        private static readonly int MasterPort =
            ApplicationSettings.GetInteger("MasterPort");

        private static readonly int SlotsCount =
            ApplicationSettings.GetInteger("SlotsCount");

        private static readonly SlaveNode Slave =
            new SlaveNode(LocalPort, LocalAddress, SlotsCount);

        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            Logger.Info("Starting Slave ...");
            ThreadPool.QueueUserWorkItem(HandleMessages, Slave);
            ThreadPool.QueueUserWorkItem(ProcessIncomingMessages, Slave);

            Logger.Info("Joining the network at {0}:{1} ...",
                MasterAddress, MasterPort);
            try
            {
                Slave.Join(MasterAddress, MasterPort);
            }
            catch
            {
                Logger.Error("Join failed.");
            }

            Logger.Info("Press Enter to quit.");
            Console.ReadLine();
            Slave.Dispose();
        }

        private static void HandleMessages(object state)
        {
            while (Slave.State != Computing.Enums.SlaveState.Left)
            {
                Slave.HandleMessages(1);
            }
        }

        private static void ProcessIncomingMessages(object state)
        {
            while (Slave.State != Computing.Enums.SlaveState.Left)
            {
                try
                {
                    Slave.ProcessIncomingMessages(1);
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
