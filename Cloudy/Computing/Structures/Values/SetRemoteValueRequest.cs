using System;
using Cloudy.Computing.Enums;
using Cloudy.Helpers;
using Cloudy.Messaging.Structures;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Structures.Values
{
    /// <summary>
    /// Used to set a value on a remote node.
    /// </summary>
    [ProtobufSerializable]
    public class SetRemoteValueRequest : ByteArrayValue
    {
        public SetRemoteValueRequest()
        {
            this.Namespace = Namespaces.Default;
        }

        /// <summary>
        /// Namespace of key. Equals to a rank when stored on a slave (and
        /// therefore affected by rank reassigning).
        /// </summary>
        [ProtobufField(2)]
        public byte[] Namespace { get; set; }

        [ProtobufField(3)]
        public string Key { get; set; }

        [ProtobufField(4)]
        public TimeToLive TimeToLive { get; set; }
    }
}
