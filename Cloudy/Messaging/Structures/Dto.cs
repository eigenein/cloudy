using System;
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
        protected readonly long trackingId;

        protected readonly T value;

        protected readonly int? tag;

        internal Dto(long trackingId, int? tag, T value)
        {
            this.trackingId = trackingId;
            this.value = value;
            this.tag = tag;
        }

        /// <summary>
        /// An ID that should be unique within the set of currently
        /// active operations. Used to track messages.
        /// </summary>
        [ProtobufField(1)]
        public long TrackingId
        {
            get { return trackingId; }
        }

        /// <summary>
        /// An user-specific tag. Can indicate a type of the message.
        /// </summary>
        [ProtobufField(2)]
        public int? Tag
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
        internal Dto(long trackingId, int? tag, byte[] value)
            : base(trackingId, tag, value)
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
            return new Dto<T>(trackingId, tag, deserializedValue);
        }
    }
}
