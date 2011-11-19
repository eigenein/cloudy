using System;
using System.Collections.Generic;
using Cloudy.Computing.Topologies.Enums;
using Cloudy.Computing.Topologies.Interfaces;
using Cloudy.Computing.Topologies.Routers;

namespace Cloudy.Computing.Topologies
{
    public static class RouterFactory
    {
        private static readonly Dictionary<TopologyType, Func<IRouter>> Cache =
            new Dictionary<TopologyType, Func<IRouter>>()
            {
                {TopologyType.Star, () => new StarRouter()}
            };

        public static IRouter Create(TopologyType topologyType)
        {
            return Cache[topologyType]();
        }
    }
}
