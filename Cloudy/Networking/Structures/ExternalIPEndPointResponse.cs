using System;
using System.Net;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Networking.Structures
{
    /// <summary>
    /// Holds an <see cref="System.Net.IPEndPoint"/> representation.
    /// </summary>
    [ProtobufSerializable]
    public class ExternalIPEndPointResponse
    {
        /// <summary>
        /// Empty constructor for deserialization.
        /// </summary>
        public ExternalIPEndPointResponse()
        {
            // Do nothing.
        }

        /// <summary>
        /// Initializes a new instance by the given endpoint.
        /// </summary>
        public ExternalIPEndPointResponse(int requestId, IPEndPoint endPoint)
        {
            RequestId = requestId;
            Port = endPoint.Port;
            IPAddress = endPoint.Address.GetAddressBytes();
        }

        /// <summary>
        /// Request ID. Set by a server to the same value as in the original request.
        /// </summary>
        [ProtobufField(1)]
        public int RequestId { get; set; }

        [ProtobufField(2)]
        public int Port { get; set; }

        [ProtobufField(3)]
        public byte[] IPAddress { get; set; }

        /// <summary>
        /// Reconstructs an <see cref="System.Net.IPEndPoint"/> instance.
        /// </summary>
        public IPEndPoint AsIPEndPoint
        {
            get { return new IPEndPoint(new IPAddress(IPAddress), Port); }
        }
    }
}
