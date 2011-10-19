using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Test.Protobuf.TestClasses
{
    [ProtobufSerializable]
    public class A
    {
        [ProtobufField(1)]
        public uint B { get; set; }
    }
}
