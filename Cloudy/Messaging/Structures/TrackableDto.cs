﻿using System;
using Cloudy.Messaging.Interfaces;
using Cloudy.Protobuf;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Messaging.Structures
{
    /// <summary>
    /// A trackable Data Transfer Object.
    /// </summary>
    [ProtobufSerializable]
    public class TrackableDto<T>
    {
        /// <summary>
        /// A parameterless constructor for deserialization.
        /// </summary>
        public TrackableDto()
        {
            // Do nothing.
        }

        public TrackableDto(long trackingId, int tag, T value)
        {
            this.Tag = tag;
            this.Value = value;
            this.TrackingId = trackingId;
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

        /// <summary>
        /// An ID that should be unique within the set of currently
        /// active operations. Used to track messages.
        /// </summary>
        [ProtobufField(3)]
        public long TrackingId { get; set; }

        /// <summary>
        /// Serializes the DTO.
        /// </summary>
        public byte[] Serialize()
        {
            return Serializer.CreateSerializer(typeof(TrackableDto<T>)).Serialize(this);
        }
    }

    /// <summary>
    /// An untyped trackable Data Transfer Object.
    /// </summary>
    [ProtobufSerializable]
    public class TrackableDto : ByteArrayValue, IMessage
    {
        /// <summary>
        /// A parameterless constructor for deserialization.
        /// </summary>
        public TrackableDto()
        {
            // Do nothing.
        }

        public TrackableDto(long trackingId, int tag, byte[] value)
        {
            this.Tag = tag;
            this.Value = value;
            this.TrackingId = trackingId;
        }

        /// <summary>
        /// An user-specific tag. Can indicate a type of the message.
        /// </summary>
        [ProtobufField(2)]
        public int Tag { get; set; }

        /// <summary>
        /// An ID that should be unique within the set of currently
        /// active operations. Used to track messages.
        /// </summary>
        [ProtobufField(3)]
        public long TrackingId { get; set; }
    }
}
