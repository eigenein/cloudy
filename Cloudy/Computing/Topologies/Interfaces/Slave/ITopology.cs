using System;
using Cloudy.Computing.Topologies.Structures;

namespace Cloudy.Computing.Topologies.Interfaces.Slave
{
    /// <summary>
    /// Incapsulates slave-side topology operations.
    /// </summary>
    public interface ITopology
    {
        Type RankType { get; }

        /// <summary>
        /// Tries to find a route path to the destination thread.
        /// </summary>
        RouteSearchResult TryFindNext(byte[] current, byte[] destination);
    }
}
