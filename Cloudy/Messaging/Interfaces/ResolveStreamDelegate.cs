using System;

namespace Cloudy.Messaging.Interfaces
{
    public delegate bool ResolveStreamDelegate(Guid id, out MessageStream stream);
}
