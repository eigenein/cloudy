using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Structures.Values
{
    [ProtobufSerializable]
    public class ReassignRankValue
    {
        [ProtobufField(1)]
        public byte[] OldRank { get; set; }

        [ProtobufField(2)]
        public byte[] NewRank { get; set; }
    }
}
