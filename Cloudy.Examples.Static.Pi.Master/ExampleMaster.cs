using System;
using Cloudy.Computing.Topologies.Enums;

namespace Cloudy.Examples.Static.Pi.Master
{
    internal class ExampleMaster : Computing.StaticMaster
    {
        public ExampleMaster(int port, int threadsCount) 
            : base(port, threadsCount)
        {
            // Do nothing.
        }

        /// <summary>
        /// Gets topologies that are used in the network.
        /// </summary>
        public override TopologyType[] UsedTopologies
        {
            get { throw new NotImplementedException(); }
        }
    }
}
