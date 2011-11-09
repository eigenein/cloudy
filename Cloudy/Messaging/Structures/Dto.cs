using System;
using Cloudy.Messaging.Interfaces;
using Cloudy.Protobuf;
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

        public Dto(int? tag, T value)
        {
            this.Value = value;
            this.Tag = tag;
        }

        /// <summary>
        /// An user-specific tag. Can indicate a type of the message.
        /// </summary>
        [ProtobufField(1)]
        public int? Tag { get; set; }

        /// <summary>
        /// Gets an underlying value.
        /// </summary>
        [ProtobufField(2)]
        public T Value { get; set; }
    }

    /// <summary>
    /// An untyped Data Transfer Object.
    /// </summary>
    public class Dto : ICastableValue
    {
        /// <summary>
        /// A parameterless constructor for deserialization.
        /// </summary>
        public Dto()
        {
            // Do nothing.
        }

        public Dto(int? tag, byte[] value)
        {
            this.Tag = tag;
            this.Value = value;
        }

        /// <summary>
        /// An user-specific tag. Can indicate a type of the message.
        /// </summary>
        [ProtobufField(1)]
        public int? Tag { get; set; }

        /// <summary>
        /// Gets an underlying value.
        /// </summary>
        [ProtobufField(2)]
        public byte[] Value { get; set; }

        /// <summary>
        /// Deserializes the underlying byte-array value into
        /// a value of the specified type.
        /// </summary>
        public T Get<T>()
        {
            return (T)Get(typeof(T));
        }

        /// <summary>
        /// Deserializes the underlying byte-array value into
        /// a value of the specified type.
        /// </summary>
        public object Get(Type type)
        {
            return Serializer.CreateSerializer(type).Deserialize(Value);
        }
    }
}
