using System;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Structures;
using Cloudy.Computing.Topologies.Interfaces;

namespace Cloudy.Computing
{
    public abstract class AbstractStaticMasterNode : AbstractMasterNode
    {
        private readonly int startUpThreadsCount;

        public AbstractStaticMasterNode(int port, int startUpThreadsCount, 
            INetworkRepository networkRepository, ITopologyRepository topologyRepository) 
            : base(port, networkRepository, topologyRepository)
        {
            this.startUpThreadsCount = startUpThreadsCount;
        }

        #region Overrides of AbstractMasterNode

        protected override bool Start()
        {
            return NetworkRepository.GetTotalSlotsCount() >= startUpThreadsCount && base.Start();
        }

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

        protected override bool OnThreadFailedToStart(Guid slaveId, Guid threadId)
        {
            return false;
        }

        #endregion
    }
}
