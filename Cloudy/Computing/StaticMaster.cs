using System;

namespace Cloudy.Computing
{
    /// <summary>
    /// Represents a master for a fixed-size network. No adding or removing
    /// of dynamic nodes are allowed.
    /// </summary>
    public abstract class StaticMaster : Master
    {
        private readonly int threadsCount;

        protected StaticMaster(int port, int threadsCount) 
            : base(port)
        {
            this.threadsCount = threadsCount;
        }

        public override int MinimumThreadsCount
        {
            get { return threadsCount; }
        }
    }
}
