using System;
using System.Net;

namespace Cloudy.Computing.Structures
{
    public class SlaveContext
    {
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

        public override string ToString()
        {
            return ExternalEndPoint.ToString();
        }
    }
}
