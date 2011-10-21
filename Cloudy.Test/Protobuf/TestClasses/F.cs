using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Test.Protobuf.TestClasses
{
    [ProtobufSerializable]
    public class F
    {
        [ProtobufField(3)]
        public A Message { get; set; }
    }
}
