using System;

namespace Cloudy.Computing.Interfaces
{
    public interface IEnvironment
    {
        Guid ThreadId { get; }
    }
}
