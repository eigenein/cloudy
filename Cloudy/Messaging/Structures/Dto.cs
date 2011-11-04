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

        public Dto(Guid fromId, long trackingId, int? tag, T value)
        {
            this.FromId = fromId;
            this.TrackingId = trackingId;
            this.Value = value;
            this.Tag = tag;
        }

        /// <summary>
        /// Gets the recipient identifier.
        /// </summary>
        [ProtobufField(1)]
        public Guid FromId { get; set; }

        /// <summary>
        /// An ID that should be unique within the set of currently
        /// active operations. Used to track messages.
        /// </summary>
        [ProtobufField(2)]
        public long TrackingId { get; set; }

        /// <summary>
        /// An user-specific tag. Can indicate a type of the message.
        /// </summary>
        [ProtobufField(3)]
        public int? Tag { get; set; }

        /// <summary>
        /// Gets an underlying value.
        /// </summary>
        [ProtobufField(4)]
        public T Value { get; set; }

        /// <summary>
        /// Converts this Data Transfer Object into an untyped one via
        /// serialization.
        /// </summary>
        public Dto AsUntyped()
        {
            return new Dto(FromId, TrackingId, Tag,
                Serializer.CreateSerializer(typeof(T)).Serialize(this));
        }
    }

    /// <summary>
    /// An untyped Data Transfer Object.
    /// </summary>
    public class Dto : Dto<byte[]>, ICastableValue
    {
        /// <summary>
        /// A parameterless constructor for deserialization.
        /// </summary>
        public Dto()
        {
            // Do nothing.
        }

        public Dto(Guid fromId, long trackingId, int? tag, byte[] value)
            : base(fromId, trackingId, tag, value)
        {
            // Do nothing.
        }

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
