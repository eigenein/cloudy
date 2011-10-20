using System;
using System.Collections.Generic;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Test.Protobuf.TestClasses
{
    [ProtobufSerializable]
    public class E
    {
        [ProtobufField(4, packed: true)]
        public ICollection<uint> List { get; set; }
    }
}
