using System;
using System.Net.Sockets;
using System.Threading;

using Cloudy.Computing.Enums;
using Cloudy.Computing.Nodes;
using Cloudy.Computing.Topologies.Interfaces.Master;
using Cloudy.Computing.Topologies.Master;
using Cloudy.Examples.Shared.Configuration;

using NLog;

namespace Cloudy.Examples.Static.MapReduce.Master
{
    public class Program : AbstractStaticMasterNode
    {
        private static readonly Logger Logger =
            LogManager.GetCurrentClassLogger();

        private readonly ITopology topology = new StarTopology();

        public Program() : base(
            ApplicationSettings.GetInteger("Port"),
            ApplicationSettings.GetInteger("StartUpThreadsCount"))
        {
            ThreadPool.QueueUserWorkItem(HandleMessagesLoop, null);
            ThreadPool.QueueUserWorkItem(ProcessIncomingMessagesLoop, null);
        }

        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            using (Program program = new Program())
            {
                program.StateChanged += (sender, e) =>
                    Logger.Info("State: {0}", e.Value);
                program.SlaveJoined += (sender, e) =>
                    Logger.Info("Slave joined: {1} at {0}.", e.Value1, e.Value2);
                program.JobStopped += (sender, e) =>
                    Logger.Info("Job stopped: {0}", e.Value);
                Logger.Info("Press Enter to quit.");
                Console.ReadLine();
            }
        }

        private void ProcessIncomingMessagesLoop(object state)
        {
            while (State != MasterState.Left)
            {
                try
                {
                    ProcessIncomingMessages(1);
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

        private void HandleMessagesLoop(object state)
        {
            while (State != MasterState.Left)
            {
                HandleMessages(1);
            }
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Error(e.ExceptionObject.ToString());
        }

        #region Overrides of AbstractMasterNode

        protected override ITopology Topology
        {
            get { return topology; }
        }

        protected override bool OnJobStopped(JobResult result)
        {
            return false;
        }

        #endregion
    }
}
