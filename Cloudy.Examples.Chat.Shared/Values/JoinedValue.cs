using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Examples.Chat.Shared.Values
{
    [ProtobufSerializable]
    public class JoinedValue
    {
        [ProtobufField(1)]
        public Guid MyId { get; set; }

        [ProtobufField(2)]
        public string Nickname { get; set; }
    }
}
