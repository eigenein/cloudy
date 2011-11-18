using System;
using Cloudy.Computing.Structures;

namespace Cloudy.Computing.Events
{
    public class SlaveLeftEventArgs : AbstractSlaveEventArgs
    {
        public SlaveLeftEventArgs(SlaveContext slaveContext) 
            : base(slaveContext)
        {
            // Do nothing.
        }
    }
}
