using System;
using Cloudy.Protobuf;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Connections.Dto
{
    /// <summary>
    /// A generic Data Transfer Object.
    /// </summary>
    [ProtobufSerializable]
    public class Dto<T>
    {
        protected readonly uint? uniqueId;

        protected readonly T value;

        protected readonly uint? tag;

        internal Dto(uint? uniqueId, uint? tag, T value)
        {
            this.uniqueId = uniqueId;
            this.value = value;
            this.tag = tag;
        }

        /// <summary>
        /// An ID that should be unique within the set of currently
        /// active operations. Used to track messages.
        /// </summary>
        [ProtobufField(1)]
        public uint? UniqueId
        {
            get { return uniqueId; }
        }

        /// <summary>
        /// An user-specific tag. Can indicate a type of the message.
        /// </summary>
        [ProtobufField(2)]
        public uint? Tag
        { 
            get { return tag; }
        }

        /// <summary>
        /// Gets an underlying value.
        /// </summary>
        [ProtobufField(3)]
        public T Value
        { 
            get { return value; }
        }
    }

    /// <summary>
    /// An untyped Data Transfer Object.
    /// </summary>
    public class Dto : Dto<byte[]>
    {
        protected Dto(uint? uniqueId, uint? tag, byte[] value)
            : base(uniqueId, tag, value)
        {
            // Do nothing.
        }

        /// <summary>
        /// Deserializes the underlying byte-array value into
        /// a value of the specified type.
        /// </summary>
        public Dto<T> ConvertTo<T>()
        {
            T deserializedValue = (T)Serializer.CreateSerializer(typeof(T)).Deserialize(value);
            return new Dto<T>(uniqueId, tag, deserializedValue);
        }
    }
}
