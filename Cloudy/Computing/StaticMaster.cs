using System;
using Cloudy.Computing.Exceptions;
using Cloudy.Computing.Structures;
using Cloudy.Computing.Topologies;

namespace Cloudy.Computing
{
    /// <summary>
    /// Represents a master for a fixed-size network. No adding or removing
    /// of dynamic nodes are allowed.
    /// </summary>
    public abstract class StaticMaster : Master
    {
        private readonly int threadsCount;

        protected StaticMaster(int port, int threadsCount, Topology topology) 
            : base(port, topology)
        {
            this.threadsCount = threadsCount;
        }

        public override int StartThreadsCount
        {
            get { return threadsCount; }
        }

        protected override void OnSlaveLeft(SlaveContext slaveContext)
        {
            base.OnSlaveLeft(slaveContext);
            foreach (ThreadContext thread in slaveContext.Threads)
            {
                if (thread.State == Enums.ThreadState.Running)
                {
                    OnJobCompleted(false, "A static master doesn't support dynamic slaves.");
                }
            }
        }
    }
}
