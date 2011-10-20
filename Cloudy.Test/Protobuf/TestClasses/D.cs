using System;
using System.Collections.Generic;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Test.Protobuf.TestClasses
{
    [ProtobufSerializable]
    public class D
    {
        [ProtobufField(1)]
        public ICollection<uint> List { get; set; }
    }
}
