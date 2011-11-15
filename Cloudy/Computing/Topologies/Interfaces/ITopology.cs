using System;
using System.Collections.Generic;
using Cloudy.Computing.Structures;

namespace Cloudy.Computing.Topologies.Interfaces
{
    public interface ITopology
    {
        IEnumerable<int[]> Allocate(SlaveContext slave, int threadsCount);

        void Free(int[] address);

        int[] GetRoute(int[] current, int[] destination);
    }
}
