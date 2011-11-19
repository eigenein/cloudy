using System;
using System.Collections.Generic;
using Cloudy.Computing.Topologies.Enums;

namespace Cloudy.Computing.Topologies
{
    public static class TopologyFactory
    {
        private static readonly Dictionary<TopologyType, Func<Topology>> Cache =
            new Dictionary<TopologyType, Func<Topology>>()
            {
                {TopologyType.Star, () => new StarTopology()}
            };

        public static Topology Create(TopologyType topologyType)
        {
            return Cache[topologyType]();
        }
    }
}
