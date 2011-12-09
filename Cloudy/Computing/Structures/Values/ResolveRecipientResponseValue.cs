using System;
using System.Collections.Generic;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Structures.Values
{
    [ProtobufSerializable]
    public class ResolveRecipientResponseValue
    {
        [ProtobufField(1)]
        public ICollection<Guid> ResolvedTo { get; set; }
    }
}
