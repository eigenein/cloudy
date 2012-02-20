using System;
using System.Collections.Generic;
using Cloudy.Computing.Enums;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Structures.Values.Environment
{
    [ProtobufSerializable]
    public class ReduceRequestValue
    {
        [ProtobufField(1)]
        public ICollection<byte[]> Participants { get; set; }

        [ProtobufField(2)]
        public ReduceOperation Operation { get; set; }
    }
}
