using System;
using Cloudy.Computing.Topologies;

namespace Cloudy.Examples.Static.Pi.Master
{
    internal class ExampleMaster : Computing.StaticMaster
    {
        public ExampleMaster(int port, int threadsCount) 
            : base(port, threadsCount, new StarTopology())
        {
            // Do nothing.
        }
    }
}
