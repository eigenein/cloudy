using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Structures.Values
{
    [ProtobufSerializable]
    public class GuidValue
    {
        [ProtobufField(1)]
        public Guid Value { get; set; }
    }
}
