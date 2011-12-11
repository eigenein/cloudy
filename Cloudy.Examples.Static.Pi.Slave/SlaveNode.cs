using System;
using System.Net;
using Cloudy.Computing;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Topologies.Shortcuts;
using NLog;

namespace Cloudy.Examples.Static.Pi.Slave
{
    public class SlaveNode : AbstractSlaveNode
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
                Logger.Info("Thread Started: {0}", e.Value);
            ThreadStopped += (sender, e) =>
                Logger.Info("Thread Stopped: {0}", e.Value);
            ExceptionUnhandled += (sender, e) =>
                Logger.Error("Unhandled Exception: {0}", e.Value.ToString());
            CreatingWormHole += (sender, e) =>
                Logger.Info("Creating a wormhole to {0} using {1}", e.Value1, e.Value2);
            PortScanning += (sender, e) =>
                Logger.Info("Trying {0} as {1}", e.Value1, e.Value2);
            SignedPingRequested += (sender, e) =>
                Logger.Info("Signed ping request to local {0}, external {1}",
                e.Value1, e.Value2);
            SignedPingFinished += (sender, e) =>
                Logger.Info("Signed ping to {0} has finished. Success: {1}",
                e.Value1, e.Value2);
            EndPointResolved += (sender, e) =>
                Logger.Info("{0} is resolved to {1}", e.Value1, e.Value2);
        }

        private static void Run(IEnvironment environment)
        {
            // TODO: Implement.
            Logger.Info("RUNNING");
            bool isCenter;
            Logger.Info("Am I center? {0}", isCenter =
                environment.ResolveId(StarShortcuts.Center).Contains(environment.ThreadId));
            bool isPeripheral;
            Logger.Info("Am I peripheral? {0}", isPeripheral =
                environment.ResolveId(StarShortcuts.Peripherals).Contains(environment.ThreadId));
            if (isPeripheral)
            {
                environment.Send(0, "Hello", StarShortcuts.Center);
            }
            if (isCenter)
            {
                Guid senderId;
                string value;
                environment.Receive(0, out value, out senderId);
                Logger.Info("{0} from {1}", value, senderId);
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
