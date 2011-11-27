using System;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Structures;

namespace Cloudy.Computing
{
    public abstract class AbstractStaticMasterNode : AbstractMasterNode
    {
        private readonly int startUpThreadsCount;

        public AbstractStaticMasterNode(int port, int startUpThreadsCount, 
            INetworkRepository networkRepository) 
            : base(port, networkRepository)
        {
            this.startUpThreadsCount = startUpThreadsCount;
        }

        #region Overrides of AbstractMasterNode

        protected override void OnSlaveJoined(SlaveContext slave)
        {
            if (State != MasterState.Running)
            {
                CreateThreads(slave);
                if (NetworkRepository.GetTotalSlotsCount() >= startUpThreadsCount)
                {
                    Start();
                }
            }
        }

        protected override void OnSlaveLeft(SlaveContext slave)
        {
            foreach (ThreadContext thread in NetworkRepository.GetThreads(slave.SlaveId))
            {
                if (thread.State == ThreadState.Running)
                {
                    StopJob(JobResult.Failed);
                }
            }
        }

        protected override void OnThreadFailedToStart(Guid slaveId, Guid threadId)
        {
            StopJob(JobResult.Failed);
        }

        #endregion
    }
}
