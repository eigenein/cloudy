using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Test.Protobuf.TestClasses
{
    [ProtobufSerializable]
    public class A
    {
        public A()
        {
            B = 666;
        }

        [ProtobufField(1)]
        public uint B { get; set; }
    }
}
