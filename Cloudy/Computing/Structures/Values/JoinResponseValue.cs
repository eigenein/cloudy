using System;
using Cloudy.Computing.Topologies.Enums;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Structures.Values
{
    [ProtobufSerializable]
    public class JoinResponseValue
    {
        [ProtobufField(1)]
        public EndPointValue ExternalEndPoint { get; set; }

        [ProtobufField(2)]
        public Guid SlaveId { get; set; }

        [ProtobufField(3)]
        public TopologyType TopologyType { get; set; }
    }
}
