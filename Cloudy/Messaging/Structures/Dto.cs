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
        private readonly Guid fromId;

        private readonly long trackingId;

        protected readonly T value;

        private readonly int? tag;

        internal Dto(Guid fromId, long trackingId, int? tag, T value)
        {
            this.fromId = fromId;
            this.trackingId = trackingId;
            this.value = value;
            this.tag = tag;
        }

        /// <summary>
        /// Gets the recipient identifier.
        /// </summary>
        public Guid FromId
        {
            get { return fromId; }
        }

        /// <summary>
        /// An ID that should be unique within the set of currently
        /// active operations. Used to track messages.
        /// </summary>
        [ProtobufField(2)]
        public long TrackingId
        {
            get { return trackingId; }
        }

        /// <summary>
        /// An user-specific tag. Can indicate a type of the message.
        /// </summary>
        [ProtobufField(3)]
        public int? Tag
        { 
            get { return tag; }
        }

        /// <summary>
        /// Gets an underlying value.
        /// </summary>
        [ProtobufField(4)]
        public T Value
        { 
            get { return value; }
        }

        /// <summary>
        /// Converts this Data Transfer Object into an untyped one via
        /// serialization.
        /// </summary>
        public Dto AsUntyped()
        {
            return new Dto(fromId, trackingId, tag,
                Serializer.CreateSerializer(typeof(T)).Serialize(this));
        }
    }

    /// <summary>
    /// An untyped Data Transfer Object.
    /// </summary>
    public class Dto : Dto<byte[]>, ICastableValueProvider
    {
        internal Dto(Guid fromId, long trackingId, int? tag, byte[] value)
            : base(fromId, trackingId, tag, value)
        {
            // Do nothing.
        }

        /// <summary>
        /// Deserializes the underlying byte-array value into
        /// a value of the specified type.
        /// </summary>
        public T GetValue<T>()
        {
            return (T)GetValue(typeof(T));
        }

        /// <summary>
        /// Deserializes the underlying byte-array value into
        /// a value of the specified type.
        /// </summary>
        public object GetValue(Type type)
        {
            return Serializer.CreateSerializer(type).Deserialize(value);
        }
    }
}
