using System;
using System.Net;
using Cloudy.Computing.Topologies.Enums;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Messaging.Structures
{
    [ProtobufSerializable]
    public class JoinResponseValue
    {
        /// <summary>
        /// Empty constructor for deserialization.
        /// </summary>
        public JoinResponseValue()
        {
            // Do nothing.
        }

        public JoinResponseValue(IPEndPoint externalEndPoint, TopologyType topologyType)
        {
            this.ExternalAddress = externalEndPoint.Address.GetAddressBytes();
            this.ExternalPort = externalEndPoint.Port;
            this.TopologyType = topologyType;
        }

        [ProtobufField(1)]
        public byte[] ExternalAddress { get; set; }

        [ProtobufField(2)]
        public int ExternalPort { get; set; }

        [ProtobufField(3)]
        public TopologyType TopologyType { get; set; }

        public IPEndPoint ExternalEndPoint
        {
            get { return new IPEndPoint(new IPAddress(ExternalAddress), ExternalPort); }
        }
    }
}
