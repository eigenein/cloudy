using System;
using System.Threading;
using Cloudy.Computing.Topologies.Enums;
using Cloudy.Computing.Topologies.Interfaces.Master;
using Cloudy.Computing.Topologies.Structures;
using Cloudy.Helpers;

namespace Cloudy.Computing.Topologies.Master
{
    public class StarTopology : ITopology
    {
        private int nextRank = -1;

        #region Implementation of ITopology

        public TopologyType TopologyType
        {
            get { return TopologyType.Star; }
        }

        public bool TryAddThread(out byte[] rank)
        {
            rank = RankConverter<StarRank>.Convert(new StarRank(
                Interlocked.Increment(ref nextRank)));
            return true;
        }

        #endregion
    }
}
