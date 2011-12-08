using System;
using Cloudy.Computing.Structures.Values;

namespace Cloudy.Computing.Interfaces
{
    /// <summary>
    /// Represents an environment of a computing thread.
    /// </summary>
    public interface IEnvironment
    {
        Guid ThreadId { get; }

        void NotifyValueReceived(EnvironmentOperationValue value);
    }
}
