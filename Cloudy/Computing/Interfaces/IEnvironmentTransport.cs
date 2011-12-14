using System;
using Cloudy.Computing.Structures.Values;

namespace Cloudy.Computing.Interfaces
{
    internal interface IEnvironmentTransport
    {
        void Send(EnvironmentOperationValue operationValue);
    }
}
