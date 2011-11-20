using System;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Topologies.Structures;

namespace Cloudy.Computing.Structures
{
    public class ThreadContext
    {
        public ThreadAddress Address { get; set; }

        public ThreadState State { get; set; }
    }
}
