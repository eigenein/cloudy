using System;
using System.Linq;
using System.Net;
using Cloudy.Computing;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Nodes;
using Cloudy.Computing.Topologies.Helpers;
using Cloudy.Computing.Topologies.Structures;
using NLog;

namespace Cloudy.Examples.Static.Pi.Slave
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
        }

        private static void Run(IEnvironment environment)
        {
            IEnvironment<StarRank> e = (IEnvironment<StarRank>)environment;
            Logger.Info("RUNNING");
            Logger.Info("Rank: {0}", e.Rank);
            Logger.Info("Greatest Rank: {0}",
                StarTopologyHelper.GetGreatestRank(environment));
            Logger.Info("Threads Count: {0}",
                StarTopologyHelper.GetThreadsCount(environment));
            if (!e.Rank.IsCentral)
            {
                e.Send(UserTags.Default, "Hello", StarRank.Central);
            }
            else
            {
                StarRank sender;
                string value;
                e.Receive(UserTags.Default, out value, out sender);
                Logger.Info("{0} from {1}", value, sender);
            }
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
