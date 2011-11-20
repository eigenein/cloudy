using System;
using System.Collections.Generic;
using System.Net;
using Cloudy.Computing.Enums;

namespace Cloudy.Computing.Structures
{
    public class SlaveContext
    {
        private readonly List<ThreadContext> threads = new List<ThreadContext>();

        /// <summary>
        /// The endpoint of the slave in its local network.
        /// </summary>
        public IPEndPoint LocalEndPoint { get; set; }

        /// <summary>
        /// The endpoint of the slave relatively to the master.
        /// </summary>
        public IPEndPoint ExternalEndPoint { get; set; }

        /// <summary>
        /// The associated metadata.
        /// </summary>
        public byte[] Metadata { get; set; }

        /// <summary>
        /// Gets or sets the available thread slots count on the slave.
        /// </summary>
        public int SlotsCount { get; set; }

        /// <summary>
        /// Gets the allocated threads.
        /// </summary>
        public List<ThreadContext> Threads
        {
            get { return threads; }
        }

        public SlaveState State { get; set; }

        public override string ToString()
        {
            return ExternalEndPoint.ToString();
        }
    }
}
