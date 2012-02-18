using System;
using System.Threading;
using Cloudy.Computing.Topologies.Enums;
using Cloudy.Computing.Topologies.Helpers;
using Cloudy.Computing.Topologies.Interfaces.Master;
using Cloudy.Computing.Topologies.Structures;

namespace Cloudy.Computing.Topologies.Master
{
    /// <summary>
    /// Master star topology implementation.
    /// </summary>
    public class StarTopology : ITopology
    {
        private int greatestRank = -1;

        #region Implementation of ITopology

        public TopologyType TopologyType
        {
            get { return TopologyType.Star; }
        }

        public bool TryAddThread(out byte[] rank)
        {
            rank = RankConverter<StarRank>.Convert(new StarRank(
                Interlocked.Increment(ref greatestRank)));
            return true;
        }

        public bool RemoveThread(byte[] rank, out byte[] replaceWith)
        {
            int index = RankConverter<StarRank>.Convert(rank).Index;
            if (greatestRank != -1)
            {
                int replacementIndex = Interlocked.Decrement(ref greatestRank) + 1;
                if (index != replacementIndex)
                {
                    // Replace the existing thread rank with the removed thread rank.
                    replaceWith = RankConverter<StarRank>.Convert(
                        new StarRank(replacementIndex));
                    return true;
                }
            }
            replaceWith = null;
            return false;
        }

        public void UpdateValues(ITopologyHelper helper)
        {
            helper.SetRemoteValue("ThreadsCount", greatestRank + 1);
            helper.SetRemoteValue("GreatestRank", new StarRank(greatestRank));
        }

        #endregion
    }
}
