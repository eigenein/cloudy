using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Test.Protobuf.TestClasses
{
    [ProtobufSerializable]
    public class B
    {
        [ProtobufField(1)]
        public string C { get; set; }
    }
}
