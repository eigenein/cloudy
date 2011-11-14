using System;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Topologies.Enums;
using Cloudy.Computing.Topologies.Structures;

namespace Cloudy.Computing.Topologies
{
    public class StarTopology : ITopology<int>
    {
        #region Implementation of ITopology<int>

        /// <summary>
        /// Gets the next node address to pass a message from the current
        /// location to the destination node.
        /// </summary>
        public ThreadAddress<int> GetNextRoute(ThreadAddress<int> current, 
            ThreadAddress<int> destination)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Implementation of ITopology

        public TopologyType TopologyType
        {
            get { return TopologyType.Star; }
        }

        #endregion
    }
}
