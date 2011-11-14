using System;
using System.Collections.Generic;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing
{
    /// <summary>
    /// Represents an abstract master node.
    /// </summary>
    public abstract class Master : Node
    {
        protected Master(int port)
            : base(port)
        {
        }

        /// <summary>
        /// When overridden, creates all the topologies needed by the specific network.
        /// </summary>
        protected abstract Dictionary<int, ITopology> CreateTopologies();
    }
}
