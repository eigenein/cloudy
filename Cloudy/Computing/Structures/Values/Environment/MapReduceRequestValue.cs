using System;
using System.Collections.Generic;

using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Structures.Values.Environment
{
    [ProtobufSerializable]
    public class MapReduceRequestValue<TValue>
    {
        [ProtobufField(1)]
        public ICollection<byte[]> Participants { get; set; }

        [ProtobufField(2)]
        public TValue Value { get; set; }
    }
}
