using System;
using System.Linq;
using System.Net;
using Cloudy.Computing;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Nodes;
using Cloudy.Computing.Reduction.Delegates;
using Cloudy.Computing.Topologies.Helpers;
using Cloudy.Computing.Topologies.Structures;
using Cloudy.Examples.Shared.Configuration;
using NLog;

namespace Cloudy.Examples.Static.Pi.Slave
{
    public class SlaveNode : AbstractSlaveNode<StarRank>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly int IntervalsCount = 
            ApplicationSettings.GetInteger("IntervalsCount");

        private readonly int slotsCount;

        public SlaveNode(int port, IPAddress localAddress, int slotsCount) 
            : base(port, localAddress)
        {
            this.slotsCount = slotsCount;

            StateChanged += (sender, e) =>
                Logger.Info("State: {0}", e.Value);
            Joined += (sender, e) =>
                Logger.Info("Joined: {0} -> {1}", e.Value1, e.Value2);
            ThreadStarted += (sender, e) =>
                Logger.Info("Thread Started: {0}", BitConverter.ToString(e.Value));
            ThreadStopped += (sender, e) =>
                Logger.Info("Thread Stopped: {0}", BitConverter.ToString(e.Value));
            ExceptionUnhandled += (sender, e) =>
                Logger.Error("Unhandled Exception: {0}", e.Value.ToString());
            CreatingWormHole += (sender, e) =>
                Logger.Info("Creating a wormhole to {0} using {1}", 
                BitConverter.ToString(e.Value1), e.Value2);
            PortScanning += (sender, e) =>
                Logger.Info("Trying {0} as {1}", e.Value1, e.Value2);
            SignedPingRequested += (sender, e) =>
                Logger.Info("Signed ping request to local {0}, external {1}",
                e.Value1, e.Value2);
            SignedPingFinished += (sender, e) =>
                Logger.Info("Signed ping to {0} has finished. Success: {1}",
                String.Join(", ", e.Value1.Select(a => BitConverter.ToString(a)).ToArray()),
                e.Value2);
            EndPointResolved += (sender, e) =>
                Logger.Info("{0} is resolved to {1}", 
                BitConverter.ToString(e.Value1), e.Value2);
            RankReassigned += (sender, e) =>
                Logger.Info("Rank reassigned: {0} -> {1}",
                BitConverter.ToString(e.Value1), BitConverter.ToString(e.Value2));
            BytesSent += (sender, e) =>
                Logger.Debug("Out: {0}", BitConverter.ToString(e.Value));
            BytesReceived += (sender, e) =>
                Logger.Debug("In:  {0}", BitConverter.ToString(e.Value));
        }

        private static void Run(IEnvironment environment)
        {
            IEnvironment<StarRank> e = (IEnvironment<StarRank>)environment;
            Logger.Info("Running");
            int threadsCount = StarTopologyHelper.GetThreadsCount(environment);
            Logger.Info("Threads Count: {0}. Rank: {1}.", threadsCount, e.Rank);
            double step = 1.0 / IntervalsCount;
            double sum = 0.0;
            for (int i = e.Rank.Index + 1; i <= IntervalsCount; i += threadsCount)
            {
                double x = step * (i - 0.5);
                sum += 4.0 / (1.0 + x * x);
            }
            double partOfPi = step * sum;
            Logger.Info("Part of Pi: {0}", partOfPi);
            Reductor reductor = (reducible1, reducible2) => reducible1.Add(reducible2);
            if (e.Rank.IsCentral)
            {
                double pi = e.Reduce(UserTags.Default, partOfPi, reductor,
                    StarTopologyHelper.GetPeripherals(environment));
                Console.WriteLine("Pi is {0}", pi);
            }
            else
            {
                e.Reduce(UserTags.Default, partOfPi, reductor);
            }
            Logger.Info("Reduce has finished.");
            Logger.Info("Elapsed time: {0}.", e.Time);
        }

        #region Overrides of AbstractSlaveNode

        public override int SlotsCount
        {
            get { return slotsCount; }
        }

        protected override IComputingThread CreateThread()
        {
            return new ComputingThread(Run);
        }

        #endregion
    }
}
