using System;

namespace Cloudy.Computing.Topologies.Interfaces.Master
{
    public interface ITopologyHelper
    {
        void SetRemoteValue<TValue>(string key, TValue value);
    }
}
