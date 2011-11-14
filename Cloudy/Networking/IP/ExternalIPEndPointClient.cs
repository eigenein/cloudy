using System;
using System.Net;
using System.Net.Sockets;
using Cloudy.Messaging;
using Cloudy.Messaging.Enums;
using Cloudy.Messaging.Interfaces;
using Cloudy.Messaging.Raw;
using Cloudy.Networking.Structures;

namespace Cloudy.Networking.IP
{
    /// <summary>
    /// Represents a client for obtaining of an external IP address and port
    /// of a client.
    /// </summary>
    public static class ExternalIPEndPointClient
    {
        private static readonly Random Random = new Random();

        /// <summary>
        /// Requests information about the client external IP endpoint.
        /// </summary>
        /// <param name="remoteEndPoint">
        /// The server address and port.
        /// </param>
        /// <param name="sourcePort">
        /// The source port number that will be used in communications further.
        /// </param>
        /// <param name="externalClientEndPoint">
        /// The external client IP endpoint or <c>null</c> if request has failed.
        /// </param>
        /// <returns>Whether the request was successful.</returns>
        public static bool RequestInformation(IPEndPoint remoteEndPoint,
            int sourcePort, out IPEndPoint externalClientEndPoint)
        {
            externalClientEndPoint = null;
            Communicator<IPEndPoint> communicator = new Communicator<IPEndPoint>(
                new UdpClientRawCommunicator(new UdpClient(sourcePort)));
            try
            {
                int requestId = Random.Next();
                communicator.SendTagged(CommonTags.ExternalIPEndPointRequest,
                    new ExternalIPEndPointRequest(requestId), remoteEndPoint);
                int? tag;
                ICastable message = communicator.ReceiveTagged(out tag);
                if (tag != CommonTags.ExternalIPEndPointResponse)
                {
                    return false;
                }
                ExternalIPEndPointResponse response = message.Cast<ExternalIPEndPointResponse>();
                if (response.RequestId != requestId)
                {
                    return false;
                }
                externalClientEndPoint = response.AsIPEndPoint;
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                communicator.Dispose();
            }
        }
    }
}
