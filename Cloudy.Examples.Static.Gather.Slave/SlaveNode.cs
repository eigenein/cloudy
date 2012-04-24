using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Cloudy.Computing;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Nodes;
using Cloudy.Computing.Topologies.Helpers;
using Cloudy.Computing.Topologies.Structures;
using Cloudy.Examples.Shared.Helpers;

using NLog;

namespace Cloudy.Examples.Static.Gather.Slave
{
    public class SlaveNode : AbstractSlaveNode<StarRank>
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
            if (e.Rank.IsCentral)
            {
                IEnumerable<int> result = e.Gather<int>(
                    StarTopologyHelper.GetPeripherals(environment));
                Logger.Info("Gathered values: {0}", String.Join(", ",
                    result.Select(i => i.ToString()).ToArray()));
            }
            else
            {
                int i = RandomExtensions.Instance.Next();
                Logger.Info("Providing the value {0}.", i);
                e.Gather(i);
            }

            Logger.Info("Gather has finished. Rank: {0}.", e.Rank);
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
