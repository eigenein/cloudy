using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using Cloudy.Helpers;
using Cloudy.Networking.Dto;
using Cloudy.Protobuf;

namespace Cloudy.Networking.IP
{
    /// <summary>
    /// Represents a client for obtaining of an external IP address and port
    /// of a client.
    /// </summary>
    public static class IPEndPointClient
    {
        /// <summary>
        /// Requests an external endpoint information from the server.
        /// </summary>
        /// <param name="serverAddress">The server address.</param>
        /// <param name="serverPortNumber">The port that the server are listening to.</param>
        /// <param name="sourcePortNumber">The source port of the client.</param>
        /// <param name="timeout">Timeout of this operation.</param>
        /// <returns>The client's external IP endpoint or <c>null</c> if failed.</returns>
        public static IPEndPoint RequestExternalEndPoint(IPAddress serverAddress,
            int serverPortNumber, int sourcePortNumber, TimeSpan timeout)
        {
            UdpClient client = new UdpClient(sourcePortNumber);
            IPEndPoint targetEndPoint = new IPEndPoint(serverAddress, serverPortNumber);
            // Send the magic datagram.
            client.Send(new byte[] { 0xAA }, 1, targetEndPoint);
            // Receive the requested information.
            IPEndPoint remoteEndPoint = null;
            byte[] dgram = new byte[0];
            Action d = () => dgram = client.Receive(ref remoteEndPoint);
            if (!InvokeHelper.CallWithTimeout(d, timeout) ||
                !remoteEndPoint.Equals(targetEndPoint))
            {
                return null;
            }
            // Parse the requested information.
            try
            {
                return ((IPEndPointDto)Serializer.CreateSerializer(
                    typeof(IPEndPointDto)).Deserialize(dgram)).AsIPEndPoint();
            }
            catch (SerializationException)
            {
                return null;
            }
        }
    }
}
