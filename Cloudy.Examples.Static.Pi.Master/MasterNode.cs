using System;
using Cloudy.Computing;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Interfaces;
using NLog;

namespace Cloudy.Examples.Static.Pi.Master
{
    public class MasterNode : AbstractStaticMasterNode
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public MasterNode(int port, int startUpThreadsCount, IMasterRepository masterRepository) 
            : base(port, startUpThreadsCount, masterRepository)
        {
            SlaveJoined += (sender, e) =>
                Logger.Info("Slave joined: {0}{2}{1}", e.Value1, e.Value2, Environment.NewLine);
            StateChanged += (sender, e) =>
                Logger.Info("State: {0}", e.Value);
            SlaveLeft += (sender, e) =>
                Logger.Info("Slave left: {0}{2}{1}", e.Value1, e.Value2, Environment.NewLine);
            JobStopped += (sender, e) =>
                Logger.Info("Job stopped: {0}", e.Value);
            ThreadFailedToStart += (sender, e) =>
                Logger.Info("Thread failed to start: {1}", e.Value2);
            FailedToStart += (sender, e) =>
                Logger.Error("Failed to start.");
            Started += (sender, e) =>
                Logger.Info("Started");
            StartingThread += (sender, e) =>
                Logger.Info("Starting thread: {0}", e.Value);
        }

        #region Overrides of AbstractMasterNode

        protected override bool OnJobStopped(JobResult result)
        {
            return false;
        }

        #endregion
    }
}
