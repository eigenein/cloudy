using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Cloudy.Computing;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Nodes;
using Cloudy.Computing.Topologies.Helpers;
using Cloudy.Computing.Topologies.Structures;
using Cloudy.Examples.Shared.Configuration;

using NLog;

namespace Cloudy.Examples.Static.MapReduce.Slave
{
    public class Program : AbstractSlaveNode<StarRank>
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

        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            using (Program program = new Program())
            {
                program.StateChanged += (sender, e) =>
                    Logger.Info("State: {0}.", e.Value);
                program.ExceptionUnhandled += (sender, e) =>
                    Logger.ErrorException("Unhandled exception: {0}", e.Value);
                program.Join(MasterAddress, MasterPort);
                Logger.Info("Press Enter to quit.");
                Console.ReadLine();
            }
        }

        public Program() : base(LocalPort, LocalAddress)
        {
            ThreadPool.QueueUserWorkItem(HandleMessagesLoop, null);
            ThreadPool.QueueUserWorkItem(ProcessIncomingMessagesLoop, null);
        }

        private void HandleMessagesLoop(object state)
        {
            while (State != SlaveState.Left)
            {
                HandleMessages(1);
            }
        }

        private void ProcessIncomingMessagesLoop(object state)
        {
            while (State != SlaveState.Left)
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

        private void Run(IEnvironment environment)
        {
            IEnvironment<StarRank> e = (IEnvironment<StarRank>)environment;
            if (e.Rank.IsCentral)
            {
                Logger.Info("Requesting MapReduce ...");
                int value = e.MapReduce<int, int>(UserTags.Default, 1,
                    StarTopologyHelper.GetPeripherals(environment));
                Logger.Info("Value: {0}.", value);
            }
            else
            {
                Logger.Info("Providing a value for MapReduce ...");
                e.MapReduce<int, int>(UserTags.Default, Map, Reduce);
            }
        }

        private int Map(int x)
        {
            Logger.Info("Map: {0}.", x);
            return 2 * x;
        }

        private void Reduce(IReducible reducible1, IReducible reducible2)
        {
            Logger.Info("Reduce: {0} op {1}", reducible1, reducible2);
            reducible1.Add(reducible2);
        }

        #region Overrides of AbstractSlaveNode<StarRank>

        public override int SlotsCount
        {
            get { return ApplicationSettings.GetInteger("SlotsCount"); }
        }

        protected override IComputingThread CreateThread()
        {
            return new ComputingThread(Run);
        }

        #endregion

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Error(e.ExceptionObject.ToString());
        }
    }
}
