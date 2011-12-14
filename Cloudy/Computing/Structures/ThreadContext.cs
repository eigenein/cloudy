using System;
using Cloudy.Computing.Enums;

namespace Cloudy.Computing.Structures
{
    public class ThreadContext
    {
        public byte[] Rank { get; set; }

        public ThreadState State { get; set; }
    }
}
