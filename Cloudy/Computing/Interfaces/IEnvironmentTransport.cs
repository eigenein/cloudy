using System;
using System.Collections.Generic;
using Cloudy.Computing.Structures.Values;

namespace Cloudy.Computing.Interfaces
{
    internal interface IEnvironmentTransport
    {
        void Send(EnvironmentOperationValue operationValue);

        ICollection<Guid> ResolveId(Guid threadId, Guid id);
    }
}
