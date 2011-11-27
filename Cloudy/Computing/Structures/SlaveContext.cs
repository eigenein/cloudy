using System;
using System.Collections.Generic;
using System.Net;

namespace Cloudy.Computing.Structures
{
    public class SlaveContext
    {
        public IPEndPoint LocalEndPoint { get; set; }

        public IPEndPoint ExternalEndPoint { get; set; }

        public Guid SlaveId { get; set; }

        public int SlotsCount { get; set; }
    }
}
