using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Examples.Chat.Shared.Values
{
    [ProtobufSerializable]
    public class SaysValue
    {
        [ProtobufField(1)]
        public string Sender { get; set; }

        [ProtobufField(2)]
        public string Message { get; set; }
    }
}
