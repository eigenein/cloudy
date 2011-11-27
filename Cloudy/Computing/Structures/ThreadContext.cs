using System;
using Cloudy.Computing.Enums;

namespace Cloudy.Computing.Structures
{
    public class ThreadContext
    {
        public Guid ThreadId { get; set; }

        public ThreadState State { get; set; }
    }
}
