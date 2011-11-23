using System;
using System.Net;
using Cloudy.Computing.Interfaces;
using NLog;

namespace Cloudy.Examples.Static.Pi.Slave
{
    internal class ExampleSlave : Computing.Slave
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly int slotsCount;

        public ExampleSlave(IPEndPoint localEndPoint, int slotsCount)
            : base(localEndPoint)
        {
            this.slotsCount = slotsCount;
            Joined += (sender, e) => Logger.Info("Joined as {0}", e.Value);
            ThreadAllocated += (sender, e) => Logger.Info("Allocated thread {0}", e.Value);
            StateChanged += (sender, e) => Logger.Warn("State => {0}", e.Value);
            InterconnectionEstablishing += (sender, e) => Logger.Info("Interconnection establishing: {0}", e.Value);
        }

        public override int SlotsCount
        {
            get { return slotsCount; }
        }

        protected override void Run(IThreadWorld world)
        {
            throw new NotImplementedException();
        }
    }
}
