using System;
using Cloudy.Computing.Topologies.Enums;
using Cloudy.Computing.Topologies.Structures;

namespace Cloudy.Computing.Interfaces
{
    public interface ITopology
    {
        TopologyType TopologyType { get; }
    }

    public interface ITopology<TAddress> : ITopology
    {
        /// <summary>
        /// Gets the next thread address to pass a message from the current
        /// location to the destination thread.
        /// </summary>
        ThreadAddress<TAddress> GetNextRoute(ThreadAddress<TAddress> current, 
            ThreadAddress<TAddress> destination);
    }
}
