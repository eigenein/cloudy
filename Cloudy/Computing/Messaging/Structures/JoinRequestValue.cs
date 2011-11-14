using System;
using System.Net;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Messaging.Structures
{
    [ProtobufSerializable]
    public class JoinRequestValue
    {
        /// <summary>
        /// Empty constructor for deserialization.
        /// </summary>
        public JoinRequestValue()
        {
            // Do nothing.
        }

        public JoinRequestValue(IPEndPoint sourceEndPoint, IPEndPoint externalEndPoint)
        {
            this.SourceAddress = sourceEndPoint.Address.GetAddressBytes();
            this.SourcePort = sourceEndPoint.Port;
            this.ExternalAddress = externalEndPoint.Address.GetAddressBytes();
            this.ExternalPort = externalEndPoint.Port;
        }

        [ProtobufField(1)]
        public byte[] SourceAddress { get; set; }

        [ProtobufField(2)]
        public int SourcePort { get; set; }

        [ProtobufField(3)]
        public byte[] ExternalAddress { get; set; }

        [ProtobufField(4)]
        public int ExternalPort { get; set; }

        public IPEndPoint SourceEndPoint
        {
            get { return new IPEndPoint(new IPAddress(SourceAddress), SourcePort); }
        }

        public IPEndPoint ExternalEndPoint
        {
            get { return new IPEndPoint(new IPAddress(ExternalAddress), ExternalPort); }
        }
    }
}
