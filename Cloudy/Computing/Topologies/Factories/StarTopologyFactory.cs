using System;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing.Topologies.Factories
{
    public class StarTopologyFactory : ITopologyFactory
    {
        #region Implementation of ITopologyFactory

        public Type ArgumentType
        {
            get { return typeof(object); }
        }

        public ITopology Create(object arg)
        {
            return new StarTopology();
        }

        #endregion
    }
}
