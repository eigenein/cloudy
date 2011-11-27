using System;
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
    }
}
