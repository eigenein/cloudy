using System;
using Cloudy.Computing.Topologies.Structures;

namespace Cloudy.Computing.Events
{
    public class ThreadAllocatedEventArgs: EventArgs
    {
        private readonly ThreadAddress threadAddress;

        public ThreadAllocatedEventArgs(ThreadAddress threadAddress)
        {
            this.threadAddress = threadAddress;
        }

        public ThreadAddress ThreadAddress
        {
            get { return threadAddress; }
        }
    }
}
