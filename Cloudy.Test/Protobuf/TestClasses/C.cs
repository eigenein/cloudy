using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Test.Protobuf.TestClasses
{
    [ProtobufSerializable]
    public class C
    {
        [ProtobufField(1, required: true)]
        public string D { get; set; }
    }
}
