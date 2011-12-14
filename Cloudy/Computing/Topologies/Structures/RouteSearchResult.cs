using System;

namespace Cloudy.Computing.Topologies.Structures
{
    public class RouteSearchResult
    {
        public RouteSearchResult(bool success, byte[] nextRank, int distance)
        {
            this.Success = success;
            this.NextRank = nextRank;
            this.Distance = distance;
        }

        public bool Success { get; private set; }

        public byte[] NextRank { get; private set; }

        public int Distance { get; private set; }
    }
}
