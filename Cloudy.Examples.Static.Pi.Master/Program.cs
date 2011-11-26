using System;
using System.Threading;
using Cloudy.Examples.Shared.Configuration;
using NLog;

namespace Cloudy.Examples.Static.Pi.Master
{
    static class Program
    {
        private static readonly Logger Logger =
            LogManager.GetCurrentClassLogger();

        private static readonly int Port = ApplicationSettings.GetInteger(
            "Port");

        private static readonly int SlotsCount = ApplicationSettings.GetInteger(
            "SlotsCount");

        static void Main(string[] args)
        {
            Logger.Info("Starting Master on port {0} ...", Port);
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            ExampleMaster master = new ExampleMaster(Port, SlotsCount);
            ThreadPool.QueueUserWorkItem(RunMaster, master);

            Logger.Info("Press Return to quit.");
            Console.ReadLine();
            Logger.Info("Quit.");
            master.ShutdownSlaves();
            master.Dispose();
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Error(e.ExceptionObject.ToString());
        }

        private static void RunMaster(object state)
        {
            Computing.Master master = (Computing.Master)state;
            while (master.State != Computing.Enums.MasterState.Left)
            {
                try
                {
                    master.ProcessIncomingMessages(1);
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex.ToString());
                    continue;
                }
            }
        }
    }
}
