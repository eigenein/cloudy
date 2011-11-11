using System;

namespace Cloudy.Messaging.Events
{
    public class EndPointMismatchedEventArgs<TEndPoint> : EventArgs
    {
        private readonly Guid id;

        private readonly TEndPoint resolvedEndPoint;

        public EndPointMismatchedEventArgs(Guid id, TEndPoint resolvedEndPoint)
        {
            this.id = id;
            this.resolvedEndPoint = resolvedEndPoint;
        }

        public TEndPoint ResolvedEndPoint
        {
            get { return resolvedEndPoint; }
        }
    }
}
