using System;
using Cloudy.Computing.Structures.Values;

namespace Cloudy.Computing.Interfaces
{
    internal interface IInternalEnvironment : IEnvironment
    {
        void NotifyValueReceived(EnvironmentOperationValue value);
    }
}
