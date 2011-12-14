using System;
using Cloudy.Computing.Topologies.Enums;

namespace Cloudy.Computing.Topologies.Interfaces.Master
{
    /// <summary>
    /// Incapsulates master-side topology operations.
    /// </summary>
    public interface ITopology
    {
        TopologyType TopologyType { get; }

        bool TryAddThread(out byte[] rank);
    }
}
