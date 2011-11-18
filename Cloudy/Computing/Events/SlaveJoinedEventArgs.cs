using System;
using Cloudy.Computing.Structures;

namespace Cloudy.Computing.Events
{
    public class SlaveJoinedEventArgs : AbstractSlaveEventArgs
    {
        public SlaveJoinedEventArgs(SlaveContext slaveContext)
            : base(slaveContext)
        {
            // Do nothing.
        }
    }
}
