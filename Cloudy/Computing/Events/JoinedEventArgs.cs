using System;
using System.Net;

namespace Cloudy.Computing.Events
{
    public class JoinedEventArgs : EventArgs
    {
        private readonly IPEndPoint externalEndPoint;

        public JoinedEventArgs(IPEndPoint externalEndPoint)
        {
            this.externalEndPoint = externalEndPoint;
        }

        public IPEndPoint ExternalEndPoint
        {
            get { return externalEndPoint; }
        }
    }
}
