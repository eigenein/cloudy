using System;
using Cloudy.Computing.Topologies.Structures;

namespace Cloudy.Computing.Structures
{
    public class ThreadContext
    {
        public ThreadAddress Address { get; set; }

        public SlaveContext SlaveContext { get; set; }
    }
}
