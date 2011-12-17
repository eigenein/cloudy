using System;
using Cloudy.Computing;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Topologies.Interfaces.Master;
using Cloudy.Computing.Topologies.Master;
using NLog;

namespace Cloudy.Examples.Static.Pi.Master
{
    public class MasterNode : AbstractStaticMasterNode
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly ITopology topology = new StarTopology();

        public MasterNode(int port, int startUpThreadsCount) 
            : base(port, startUpThreadsCount)
        {
            SlaveJoined += (sender, e) =>
                Logger.Info("Slave joined: {0} -> {1}", e.Value1, e.Value2);
            StateChanged += (sender, e) =>
                Logger.Info("State: {0}", e.Value);
            SlaveLeft += (sender, e) =>
                Logger.Info("Slave left: {0} -> {1}", e.Value1, e.Value2);
            JobStopped += (sender, e) =>
                Logger.Info("Job stopped: {0}", e.Value);
            ThreadFailedToStart += (sender, e) =>
                Logger.Info("Thread failed to start: {0}", BitConverter.ToString(e.Value2));
            JobFailedToStart += (sender, e) =>
                Logger.Error("Failed to start.");
            Started += (sender, e) =>
                Logger.Info("Started");
            StartingThread += (sender, e) =>
                Logger.Info("Starting thread: {0}", BitConverter.ToString(e.Value));
            ThreadCompleted += (sender, e) =>
                Logger.Info("Thread Completed: {0}", BitConverter.ToString(e.Value));
            ThreadFailed += (sender, e) =>
                Logger.Error("Thread Failed: {0}", BitConverter.ToString(e.Value));
            RankReassigned += (sender, e) =>
                Logger.Info("Rank reassigned: {0} -> {1}",
                BitConverter.ToString(e.Value1), BitConverter.ToString(e.Value2));
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
