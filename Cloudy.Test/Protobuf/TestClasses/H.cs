using System;
using Cloudy.Protobuf.Attributes;
using Cloudy.Protobuf.Enums;

namespace Cloudy.Test.Protobuf.TestClasses
{
    [ProtobufSerializable]
    public class H
    {
        [ProtobufField(2, dataType: DataType.FixedInt32)]
        public int Fixed32 { get; set; }
    }
}
