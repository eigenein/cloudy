using System;

namespace Cloudy.Computing.Topologies.Enums
{
    public enum TopologyType
    {
        /// <summary>
        /// For error detecting purposes only. Shouldn't be used.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// A user-defined topology.
        /// </summary>
        Custom = 1,

        Star = 2
    }
}