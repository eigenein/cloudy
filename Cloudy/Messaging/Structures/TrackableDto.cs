using System;
using Cloudy.Messaging.Interfaces;
using Cloudy.Protobuf;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Messaging.Structures
{
    /// <summary>
    /// A trackable Data Transfer Object.
    /// </summary>
    [ProtobufSerializable]
    public class TrackableDto<T> : Dto<T>, ITrackable
    {
        /// <summary>
        /// A parameterless constructor for deserialization.
        /// </summary>
        public TrackableDto()
        {
            // Do nothing.
        }

        public TrackableDto(Guid fromId, long trackingId, int? tag, T value)
            : base(tag, value)
        {
            this.TrackingId = trackingId;
            this.FromId = fromId;
        }

        /// <summary>
        /// Gets the recipient identifier.
        /// </summary>
        [ProtobufField(3)]
        public Guid FromId { get; set; }

        /// <summary>
        /// An ID that should be unique within the set of currently
        /// active operations. Used to track messages.
        /// </summary>
        [ProtobufField(4)]
        public long TrackingId { get; set; }

        /// <summary>
        /// Serializes an underlying value.
        /// </summary>
        public TrackableDto AsTrackableDto()
        {
            return new TrackableDto(FromId, TrackingId, Tag,
                Serializer.CreateSerializer(typeof(TrackableDto<T>)).Serialize(this));
        }
    }

    /// <summary>
    /// An untyped trackable Data Transfer Object.
    /// </summary>
    public class TrackableDto : Dto, ITrackable
    {
        /// <summary>
        /// A parameterless constructor for deserialization.
        /// </summary>
        public TrackableDto()
        {
            // Do nothing.
        }

        public TrackableDto(Guid fromId, long trackingId, int? tag, byte[] value)
            : base(tag, value)
        {
            this.TrackingId = trackingId;
            this.FromId = fromId;
        }

        /// <summary>
        /// Gets the recipient identifier.
        /// </summary>
        [ProtobufField(3)]
        public Guid FromId { get; set; }

        /// <summary>
        /// An ID that should be unique within the set of currently
        /// active operations. Used to track messages.
        /// </summary>
        [ProtobufField(4)]
        public long TrackingId { get; set; }
    }
}
