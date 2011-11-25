using System;
using Cloudy.Computing.Topologies;
using NLog;

namespace Cloudy.Examples.Static.Pi.Master
{
    internal class ExampleMaster : Computing.StaticMaster
    {
        private static readonly Logger Logger =
            LogManager.GetCurrentClassLogger();

        public ExampleMaster(int port, int threadsCount) 
            : base(port, threadsCount, new StarTopology())
        {
            SlaveJoined += (sender, e) => Logger.Info("Slave joined: {0}", e.Value);
            SlaveLeft += (sender, e) => Logger.Warn("Slave left: {0}", e.Value);
            ThreadsAllocated += (sender, e) => Logger.Info("Threads Allocated: {0}", e.Value);
            SlavesCleanedUp += (sender, e) => Logger.Warn("Slaves Cleaned Up: {0}", e.Value);
            StateChanged += (sender, e) => Logger.Warn("State => {0}", e.Value);
            InterconnectionsSetUp += (sender, e) => Logger.Info("Interconnections Set Up");
            ThreadsRun += (sender, e) => Logger.Info("Threads run: {0}", e.Value);
        }
    }
}
