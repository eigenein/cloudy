using System;
using Cloudy.Computing.Structures.Values;

namespace Cloudy.Computing.Interfaces
{
    internal interface IInternalEnvironment : IEnvironment
    {
        byte[] RawRank { get; set; }

        void NotifyValueReceived(EnvironmentOperationValue value);
    }
}
