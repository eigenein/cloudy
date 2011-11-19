using System;
using Cloudy.Computing.Topologies.Interfaces;
using Cloudy.Computing.Topologies.Structures;

namespace Cloudy.Computing.Topologies.Routers
{
    public class StarRouter : IRouter
    {
        #region Implementation of IRouter

        public ThreadAddress GetRoute(ThreadAddress current, ThreadAddress destination)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
