using System;
using Cloudy.Computing.Structures;

namespace Cloudy.Computing.Events
{
    public abstract class AbstractSlaveEventArgs : EventArgs
    {
        private readonly SlaveContext slaveContext;

        protected AbstractSlaveEventArgs(SlaveContext slaveContext)
        {
            this.slaveContext = slaveContext;
        }

        public SlaveContext SlaveContext
        {
            get { return slaveContext; }
        }
    }
}
