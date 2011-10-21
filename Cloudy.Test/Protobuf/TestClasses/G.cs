using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Test.Protobuf.TestClasses
{
    [ProtobufSerializable]
    public class G
    {
        [ProtobufField(1)]
        public uint A1 { get; set; }

        [ProtobufField(2)]
        public A A2 { get; set; }

        [ProtobufField(3)]
        public uint A3 { get; set; }
    }
}
