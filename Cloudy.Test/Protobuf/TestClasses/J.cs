using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Test.Protobuf.TestClasses
{
    [ProtobufSerializable]
    public class J
    {
        [ProtobufField(1)]
        public TestEnum Enum { get; set; }
    }
}
