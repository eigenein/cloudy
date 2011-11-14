using System;

namespace Cloudy.Computing.Interfaces
{
    public interface ITopologyFactory
    {
        Type ArgumentType { get; }

        ITopology Create(object arg);
    }
}
