using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Examples.Chat.Shared.Values
{
    [ProtobufSerializable]
    public class SaysValue
    {
        [ProtobufField(1)]
        public string Message { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
