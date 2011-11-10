using System;
using System.Net;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Networking.Dto
{
    /// <summary>
    /// Holds an <see cref="System.Net.IPEndPoint"/> representation.
    /// </summary>
    [ProtobufSerializable]
    public class IPEndPointDto
    {
        /// <summary>
        /// Empty constructor for deserialization.
        /// </summary>
        public IPEndPointDto()
        {
            // Do nothing.
        }

        /// <summary>
        /// Initializes a new instance by the given endpoint.
        /// </summary>
        public IPEndPointDto(IPEndPoint endPoint)
        {
            PortNumber = endPoint.Port;
            IPAddress = endPoint.Address.GetAddressBytes();
        }

        [ProtobufField(1)]
        public int PortNumber { get; set; }

        [ProtobufField(2)]
        public byte[] IPAddress { get; set; }

        /// <summary>
        /// Reconstructs an <see cref="System.Net.IPEndPoint"/> instance.
        /// </summary>
        public IPEndPoint AsIPEndPoint()
        {
            return new IPEndPoint(new IPAddress(IPAddress), PortNumber);
        }
    }
}
