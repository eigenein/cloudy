using System;
using System.Net;
using System.Net.Sockets;
using Cloudy.Messaging;
using Cloudy.Messaging.Enums;
using Cloudy.Messaging.Interfaces;
using Cloudy.Messaging.Raw;
using Cloudy.Networking.Events;
using Cloudy.Networking.Structures;

namespace Cloudy.Networking.IP
{
    /// <summary>
    /// Represents a service that tells to a client its (client's) external
    /// IP address and port.
    /// </summary>
    public class ExternalIPEndPointServer : IDisposable
    {
        private readonly Communicator<IPEndPoint> communicator;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="port">The port to listen to.</param>
        public ExternalIPEndPointServer(int port)
        {
            this.communicator = new Communicator<IPEndPoint>(
                new UdpClientRawCommunicator(new UdpClient(port)));
        }

        /// <summary>
        /// Occurs when non-request or invalid request is received.
        /// </summary>
        public event EventHandler InvalidRequestReceived;

        /// <summary>
        /// Occurs when a correct request are received.
        /// </summary>
        public event EventHandler<ExternalIPEndPointRequestedEventArgs> ExternalIPEndPointRequested;

        /// <summary>
        /// Processes incoming requests.
        /// </summary>
        public int ProcessIncomingRequests(int count)
        {
            int processedRequestsCount = 0;
            while (processedRequestsCount < count)
            {
                int? tag;
                IPEndPoint remoteEndPoint;
                ICastable message = communicator.ReceiveTagged(out tag, out remoteEndPoint);
                if (tag == CommonTags.ExternalIPEndPointRequest)
                {
                    ExternalIPEndPointRequest request = message.Cast<ExternalIPEndPointRequest>();
                    OnExternalIPEndPointRequested(remoteEndPoint);
                    communicator.SendTagged(CommonTags.ExternalIPEndPointResponse,
                        new ExternalIPEndPointResponse(request.Id, remoteEndPoint), remoteEndPoint);
                }
                else
                {
                    OnInvalidRequestReceived();
                }
                processedRequestsCount += 1;
            }
            return processedRequestsCount;
        }

        private void OnInvalidRequestReceived()
        {
            EventHandler handler = InvalidRequestReceived;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        private void OnExternalIPEndPointRequested(IPEndPoint endPoint)
        {
            EventHandler<ExternalIPEndPointRequestedEventArgs> handler =
                ExternalIPEndPointRequested;
            if (handler != null)
            {
                handler(this, new ExternalIPEndPointRequestedEventArgs(endPoint));
            }
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                communicator.Dispose();
            }
        }
    }
}
