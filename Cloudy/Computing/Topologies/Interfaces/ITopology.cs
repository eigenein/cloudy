using System;
using Cloudy.Computing.Structures;
using Cloudy.Computing.Topologies.Structures;

namespace Cloudy.Computing.Topologies.Interfaces
{
    public interface ITopology
    {
        RelativeAddress[] Allocate(SlaveContext slave, int threadsCount);

        void Free(RelativeAddress address);

        RelativeAddress GetRoute(RelativeAddress current, RelativeAddress destination);
    }
}
