using System;
using Cloudy.Computing.Structures;
using Cloudy.Computing.Topologies.Interfaces;
using Cloudy.Computing.Topologies.Structures;

namespace Cloudy.Computing.Topologies
{
    public class StarTopology : ITopology
    {
        #region Implementation of ITopology

        public RelativeAddress[] Allocate(SlaveContext slave, int threadsCount)
        {
            throw new NotImplementedException();
        }

        public void Free(RelativeAddress address)
        {
            throw new NotImplementedException();
        }

        public RelativeAddress GetRoute(RelativeAddress current, RelativeAddress destination)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
