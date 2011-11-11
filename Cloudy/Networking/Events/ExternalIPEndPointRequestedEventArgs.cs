using System;
using System.Net;

namespace Cloudy.Networking.Events
{
    public class ExternalIPEndPointRequestedEventArgs : EventArgs
    {
        private readonly IPEndPoint endPoint;

        public ExternalIPEndPointRequestedEventArgs(IPEndPoint endPoint)
        {
            this.endPoint = endPoint;
        }

        public IPEndPoint EndPoint
        {
            get { return endPoint; }
        }
    }
}
