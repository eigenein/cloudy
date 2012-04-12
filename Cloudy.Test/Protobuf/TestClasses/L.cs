using System;

using Cloudy.Protobuf.Attributes;

namespace Cloudy.Test.Protobuf.TestClasses
{
    [ProtobufSerializable]
    class L
    {
        [ProtobufField(1)]
        public K Value { get; set; }

        [ProtobufField(2)]
        public int Tag { get; set; }

        [ProtobufField(3)]
        public long TrackingId { get; set; }
    }
}
