using System;
using System.Collections.Generic;
using System.Net;

namespace Cloudy.Computing.Structures
{
    public class SlaveContext
    {
        public SlaveContext()
        {
            Threads = new List<ThreadContext>();
        }

        public IPEndPoint LocalEndPoint { get; set; }

        public IPEndPoint ExternalEndPoint { get; set; }

        public Guid SlaveId { get; set; }

        public int SlotsCount { get; set; }

        public List<ThreadContext> Threads { get; set; }
    }
}
