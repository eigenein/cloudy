using System;
using Cloudy.Computing.Enums;
using Cloudy.Messaging.Structures;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Structures.Values
{
    [ProtobufSerializable]
    public class GetRemoteValueResponse : ByteArrayValue
    {
        /// <summary>
        /// Whether the requested key was found. The <c>null</c> value means
        /// the same as <c>true</c>.
        /// </summary>
        [ProtobufField(2)]
        public bool? Success { get; set; }

        [ProtobufField(3)]
        public TimeToLive TimeToLive { get; set; }
    }
}
