using System;
using System.Net;
using System.Net.Sockets;
using Cloudy.Helpers;
using Cloudy.Messaging;
using Cloudy.Messaging.Enums;
using Cloudy.Messaging.Interfaces;
using Cloudy.Networking.Values;

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
        /// <param name="serverEndPoint">
        /// The server address and port.
        /// </param>
        /// <param name="clientPortNumber">
        /// The source port number that will be used in communications further.
        /// </param>
        /// <param name="externalClientEndPoint">
        /// The external client IP endpoint or <c>null</c> if request has failed.
        /// </param>
        /// <returns>Whether the request was successful.</returns>
        public static bool RequestInformation(IPEndPoint serverEndPoint,
            int clientPortNumber, out IPEndPoint externalClientEndPoint)
        {
            externalClientEndPoint = null;
            Communicator<IPEndPoint> communicator = new Communicator<IPEndPoint>(
                new UdpClientRawCommunicator(new UdpClient(clientPortNumber)));
            try
            {
                int requestId = Random.Next();
                communicator.SendTagged(WellKnownTags.ExternalIPEndPointRequest,
                    new ExternalIPEndPointRequest(requestId), serverEndPoint);
                int? tag;
                ICastable message = communicator.ReceiveTagged(out tag);
                if (tag != WellKnownTags.ExternalIPEndPointResponse)
                {
                    return false;
                }
                ExternalIPEndPointResponse response = message.Cast<ExternalIPEndPointResponse>();
                if (response.RequestId != requestId)
                {
                    return false;
                }
                externalClientEndPoint = response.AsIPEndPoint();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                communicator.Close();
            }
        }
    }
}
