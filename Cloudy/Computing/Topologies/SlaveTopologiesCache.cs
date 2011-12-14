using System;
using System.Collections.Generic;
using Cloudy.Computing.Topologies.Enums;
using Cloudy.Computing.Topologies.Interfaces.Slave;
using Cloudy.Computing.Topologies.Slave;

namespace Cloudy.Computing.Topologies
{
    internal static class SlaveTopologiesCache
    {
        private static readonly Dictionary<TopologyType, ITopology> Cache =
            new Dictionary<TopologyType, ITopology>()
            {
                {TopologyType.Star, new StarTopology()}
            };

        public static ITopology GetTopology(TopologyType type)
        {
            return Cache[type];
        }
    }
}
