using System;

namespace Cloudy.Messaging.Delegates
{
    public delegate bool ResolveEndPointDelegate<TEndPoint>(Guid id,
        out TEndPoint endPoint);
}
