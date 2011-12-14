using System;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Structures;

namespace Cloudy.Computing
{
    public abstract class AbstractStaticMasterNode : AbstractMasterNode
    {
        private readonly int startUpThreadsCount;

        protected AbstractStaticMasterNode(int port, int startUpThreadsCount) 
            : base(port)
        {
            this.startUpThreadsCount = startUpThreadsCount;
        }

        #region Overrides of AbstractMasterNode

        protected override bool Start()
        {
            return TotalSlotsCount >= startUpThreadsCount && base.Start();
        }

        protected override void OnSlaveJoined(SlaveContext slave)
        {
            if (State != MasterState.Running)
            {
                CreateThreads(slave);
                if (TotalSlotsCount >= startUpThreadsCount)
                {
                    Start();
                }
            }
        }

        protected override bool OnSlaveLeft(SlaveContext slave)
        {
            foreach (ThreadContext thread in slave.Threads)
            {
                if (thread.State == ThreadState.Running)
                {
                    return false;
                }
            }
            return true;
        }

        protected override bool OnThreadFailedToStart(Guid slaveId, byte[] threadRank)
        {
            return false;
        }

        #endregion
    }
}
