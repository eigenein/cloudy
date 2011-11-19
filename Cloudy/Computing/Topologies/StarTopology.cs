using System;
using System.Collections.Generic;
using Cloudy.Computing.Topologies.Enums;
using Cloudy.Computing.Topologies.Structures;

namespace Cloudy.Computing.Topologies
{
    public class StarTopology : Topology
    {
        #region Overrides of Topology

        public override TopologyType TopologyType
        {
            get { return TopologyType.Star; }
        }

        public override ThreadAddress Allocate()
        {
            throw new NotImplementedException();
        }

        public override void Reset()
        {
            throw new NotImplementedException();
        }

        public override IEnumerator<ThreadAddress> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
