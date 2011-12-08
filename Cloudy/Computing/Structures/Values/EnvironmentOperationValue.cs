using System;
using System.Collections.Generic;
using Cloudy.Computing.Enums;
using Cloudy.Messaging.Structures;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Structures.Values
{
    [ProtobufSerializable]
    public class EnvironmentOperationValue : ByteArrayValue
    {
        /// <summary>
        /// Used to distinguish one operation from another.
        /// </summary>
        [ProtobufField(2)]
        public int OperationId { get; set; }

        [ProtobufField(3)]
        public EnvironmentOperationType OperationType { get; set; }

        [ProtobufField(4)]
        public int UserTag { get; set; }

        [ProtobufField(5)]
        public Guid SenderId { get; set; }

        [ProtobufField(6)]
        public ICollection<Guid> RecipientsIds { get; set; }
    }
}
