using System;
using Cloudy.Messaging.Interfaces;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Messaging.Structures
{
    /// <summary>
    /// A generic Data Transfer Object.
    /// </summary>
    [ProtobufSerializable]
    public class Dto<T>
    {
        /// <summary>
        /// A parameterless constructor for deserialization.
        /// </summary>
        public Dto()
        {
            // Do nothing.
        }

        public Dto(int tag, T value)
        {
            this.Value = value;
            this.Tag = tag;
        }

        /// <summary>
        /// Gets an underlying value.
        /// </summary>
        [ProtobufField(1)]
        public T Value { get; set; }

        /// <summary>
        /// An user-specific tag. Can indicate a type of the message.
        /// </summary>
        [ProtobufField(2)]
        public int Tag { get; set; }
    }

    /// <summary>
    /// An untyped Data Transfer Object.
    /// </summary>
    [ProtobufSerializable]
    public class Dto : ByteArrayValue, IMessage
    {
        /// <summary>
        /// A parameterless constructor for deserialization.
        /// </summary>
        public Dto()
        {
            // Do nothing.
        }

        public Dto(int tag, byte[] value)
        {
            this.Tag = tag;
            this.Value = value;
        }

        /// <summary>
        /// An user-specific tag. Can indicate a type of the message.
        /// </summary>
        [ProtobufField(2)]
        public int Tag { get; set; }
    }
}
