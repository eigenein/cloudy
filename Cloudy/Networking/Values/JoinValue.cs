using System;
using System.Net;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Networking.Values
{
    /// <summary>
    /// Represents a request for joining.
    /// </summary>
    [ProtobufSerializable]
    public class JoinValue
    {
        /// <summary>
        /// Empty constructor for deserialization.
        /// </summary>
        public JoinValue()
        {
            // Do nothing.
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id">The ID of a sender.</param>
        /// <param name="sourceEndPoint">The source (local) endpoint of a sender.</param>
        /// <param name="externalEndPoint">The external endpoint of a sender.</param>
        public JoinValue(Guid id, IPEndPoint sourceEndPoint, IPEndPoint externalEndPoint)
        {
            this.Id = id;
            this.SourceAddress = sourceEndPoint.Address.GetAddressBytes();
            this.SourcePortNumber = sourceEndPoint.Port;
            this.ExternalAddress = externalEndPoint.Address.GetAddressBytes();
            this.ExternalPortNumber = externalEndPoint.Port;
        }

        [ProtobufField(1)]
        public Guid Id { get; set; }

        [ProtobufField(2)]
        public byte[] SourceAddress { get; set; }

        [ProtobufField(3)]
        public int SourcePortNumber { get; set; }

        /// <summary>
        /// Reconstructs a source <see cref="System.Net.IPEndPoint"/> instance.
        /// </summary>
        public IPEndPoint SourceEndPoint
        {
            get { return new IPEndPoint(new IPAddress(SourceAddress), SourcePortNumber); }
        }

        [ProtobufField(4)]
        public byte[] ExternalAddress { get; set; }

        [ProtobufField(5)]
        public int ExternalPortNumber { get; set; }

        /// <summary>
        /// Reconstructs an external <see cref="System.Net.IPEndPoint"/> instance.
        /// </summary>
        public IPEndPoint ExternalEndPoint
        {
            get { return new IPEndPoint(new IPAddress(ExternalAddress), ExternalPortNumber); }
        }
    }
}
