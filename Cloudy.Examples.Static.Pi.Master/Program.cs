using System;
using System.Net.Sockets;
using System.Threading;
using Cloudy.Computing.Nodes;
using Cloudy.Examples.Shared.Configuration;
using NLog;

namespace Cloudy.Examples.Static.Pi.Master
{
    static class Program
    {
        private static readonly Logger Logger =
            LogManager.GetCurrentClassLogger();

        private static readonly int Port =
            ApplicationSettings.GetInteger("Port");

        private static readonly int StartUpThreadsCount =
            ApplicationSettings.GetInteger("StartUpThreadsCount");

        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            Logger.Info("Starting master on port {0} ...", Port);
            MasterNode master = new MasterNode(Port, StartUpThreadsCount);
            ThreadPool.QueueUserWorkItem(HandleMessages, master);
            ThreadPool.QueueUserWorkItem(ProcessIncomingMessages, master);
            Logger.Info("Awaiting for at least {0} threads to join ...",
                StartUpThreadsCount);

            Logger.Info("Press Enter to quit.");
            Console.ReadLine();
            master.Dispose();
        }

        private static void ProcessIncomingMessages(object state)
        {
            AbstractMasterNode master = (AbstractMasterNode)state;
            while (master.State != Computing.Enums.MasterState.Left)
            {
                try
                {
                    master.ProcessIncomingMessages(1);
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

        private static void HandleMessages(object state)
        {
            AbstractMasterNode master = (AbstractMasterNode)state;
            while (master.State != Computing.Enums.MasterState.Left)
            {
                master.HandleMessages(1);
            }
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Error(e.ExceptionObject.ToString());
        }
    }
}
