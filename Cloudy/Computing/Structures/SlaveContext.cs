using System;
using System.Collections.Generic;
using System.Net;

namespace Cloudy.Computing.Structures
{
    public class SlaveContext
    {
        private readonly object synchronizationRoot = new object();

        public SlaveContext()
        {
            Threads = new List<ThreadContext>();
        }

        public IPEndPoint LocalEndPoint { get; set; }

        public IPEndPoint ExternalEndPoint { get; set; }

        public Guid SlaveId { get; set; }

        public int SlotsCount { get; set; }

        public List<ThreadContext> Threads { get; set; }

        /// <summary>
        /// Used when sending or receiving messages to or from the slave.
        /// </summary>
        public object SynchonizationRoot
        {
            get { return synchronizationRoot; }
        }
    }
}
