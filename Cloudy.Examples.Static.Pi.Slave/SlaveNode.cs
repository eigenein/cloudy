using System;
using System.Net;
using Cloudy.Computing;
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
                Logger.Info("Joined: {0}{2}{1}", e.Value1, e.Value2, Environment.NewLine);
            ThreadStarted += (sender, e) =>
                Logger.Info("Thread Started: {0}", e.Value);
            ThreadStopped += (sender, e) =>
                Logger.Info("Thread Stopped: {0}", e.Value);
        }

        #region Overrides of AbstractSlaveNode

        public override int SlotsCount
        {
            get { return slotsCount; }
        }

        #endregion
    }
}
