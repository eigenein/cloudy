using System;

namespace Cloudy.Computing.Topologies.Interfaces.Master
{
    public interface ITopologyHelper
    {
        /// <summary>
        /// Sets a topology-related value.
        /// </summary>
        void SetTopologyRemoteValue<TValue>(string key, TValue value);
    }
}
