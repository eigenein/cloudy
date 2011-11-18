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

        private static readonly int ThreadsCount = ApplicationSettings.GetInteger(
            "ThreadsCount");

        static void Main(string[] args)
        {
            Logger.Info("Starting Master on port {0} ...", Port);
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            ExampleMaster master = new ExampleMaster(Port, ThreadsCount);
            master.SlaveJoined += (sender, e) => Logger.Info("Slave joined: {0}", e.SlaveContext);
            master.SlaveLeft += (sender, e) => Logger.Info("Slave left: {0}", e.SlaveContext);
            ThreadPool.QueueUserWorkItem(RunMaster, master);

            Logger.Info("Press Return to quit.");
            Console.ReadLine();
            Logger.Info("Quit.");
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
                master.ProcessIncomingMessages(1);
            }
        }
    }
}
