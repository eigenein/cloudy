using System;
using System.Collections.Generic;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Structures.Values
{
    [ProtobufSerializable]
    public class SignedPingValue
    {
        [ProtobufField(1)]
        public ICollection<byte[]> LocalRanks { get; set; }
    }
}
