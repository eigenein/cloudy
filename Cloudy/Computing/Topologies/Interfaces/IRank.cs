using System;
using Cloudy.Computing.Topologies.Enums;

namespace Cloudy.Computing.Topologies.Interfaces
{
    /// <summary>
    /// Represents a thread rank within a network.
    /// </summary>
    public interface IRank
    {
        TopologyType TopologyType { get; }
    }
}
