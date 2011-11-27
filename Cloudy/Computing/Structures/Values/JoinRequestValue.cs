using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Structures.Values
{
    [ProtobufSerializable]
    public class JoinRequestValue
    {
        [ProtobufField(1)]
        public EndPointValue LocalEndPoint { get; set; }

        [ProtobufField(2)]
        public int SlotsCount { get; set; }

        [ProtobufField(3)]
        public Guid? SlaveId { get; set; }
    }
}
