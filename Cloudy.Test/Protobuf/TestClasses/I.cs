using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Test.Protobuf.TestClasses
{
    [ProtobufSerializable]
    public class I
    {
        [ProtobufField(1)]
        public uint? NullableProperty { get; set; }
    }
}
