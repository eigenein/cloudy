using System;
using System.Collections.Generic;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Topologies.Enums;
using Cloudy.Computing.Topologies.Factories;

namespace Cloudy.Computing.Topologies
{
    public static class TopologyFactoryCache
    {
        private static readonly Dictionary<TopologyType, ITopologyFactory> Cache =
            new Dictionary<TopologyType, ITopologyFactory>()
            {
                {TopologyType.Star, new StarTopologyFactory()}
            };

        public static ITopology CreateTopology(TopologyType topologyType, 
            Func<Type, object> getArgumentFunc)
        {
            ITopologyFactory factory = Cache[topologyType];
            return factory.Create(getArgumentFunc(factory.ArgumentType));
        }
    }
}
