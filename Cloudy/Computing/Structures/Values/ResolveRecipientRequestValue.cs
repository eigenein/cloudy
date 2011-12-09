using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Structures.Values
{
    [ProtobufSerializable]
    public class ResolveRecipientRequestValue
    {
        [ProtobufField(1)]
        public Guid CurrentThreadId { get; set; }

        [ProtobufField(2)]
        public Guid RecipientId { get; set; }
    }
}
