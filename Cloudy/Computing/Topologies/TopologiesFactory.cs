using System;
using System.Collections.Generic;
using Cloudy.Computing.Topologies.Enums;
using Cloudy.Computing.Topologies.Interfaces;

namespace Cloudy.Computing.Topologies
{
    public static class TopologiesFactory
    {
        private static readonly Dictionary<TopologyType, Func<ITopology>> Cache =
            new Dictionary<TopologyType, Func<ITopology>>()
            {
                {TopologyType.Star, () => new StarTopology()}
            };

        public static ITopology Create(TopologyType topologyType)
        {
            return Cache[topologyType]();
        }
    }
}
