using System;
using Cloudy.Computing.Topologies.Interfaces.Slave;
using Cloudy.Computing.Topologies.Structures;
using Cloudy.Helpers;

namespace Cloudy.Computing.Topologies.Slave
{
    /// <summary>
    /// Slave star topology implementation.
    /// </summary>
    public class StarTopology : ITopology
    {
        #region Implementation of ITopology

        public Type RankType
        {
            get { return typeof(StarRank); }
        }

        public RouteSearchResult TryFindNext(byte[] current, byte[] destination)
        {
            StarRank currentStarRank = RankConverter<StarRank>.Convert(current);
            if (currentStarRank.IsCentral)
            {
                return new RouteSearchResult(true, destination, 1);
            }
            return new RouteSearchResult(true, RankConverter<StarRank>.Convert(
                StarRank.Central), 2);
        }

        #endregion
    }
}
