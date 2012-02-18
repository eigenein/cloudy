using System;
using System.Collections.Generic;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Topologies.Structures;
using Cloudy.Helpers;

namespace Cloudy.Computing.Topologies.Helpers
{
    public static class StarTopologyHelper
    {
        public static StarRank GetGreatestRank(IEnvironment environment)
        {
            StarRank rank;
            if (!environment.TryGetRemoteValue(Namespaces.Default, "Topology.GreatestRank", 
                out rank))
            {
                throw new KeyNotFoundException("The required key was not found on the master");
            }
            return rank;
        }

        public static int GetThreadsCount(IEnvironment environment)
        {
            int threadsCount;
            if (!environment.TryGetRemoteValue(Namespaces.Default, "Topology.ThreadsCount",
                out threadsCount))
            {
                throw new KeyNotFoundException("The required key was not found on the master");
            }
            return threadsCount;
        }
    }
}
