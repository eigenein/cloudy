using System;
using Cloudy.Protobuf;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Structures.Values
{
    /// <summary>
    /// Wraps a primitive value in order to simplify transferring them
    /// with an environment operation.
    /// </summary>
    [ProtobufSerializable]
    public class WrappedValue<T>
    {
        /// <summary>
        /// Empty constructor for deserialization.
        /// </summary>
        public WrappedValue()
        {
            // Do nothing.
        }

        public WrappedValue(T value)
        {
            this.Value = value;
        }

        [ProtobufField(1)]
        public T Value { get; set; }

        /// <summary>
        /// Gets the value as serialized into an array.
        /// </summary>
        /// <remarks>
        /// Do not read this property twice on the same value, because this
        /// will serialize this object twice.
        /// </remarks>
        public byte[] AsByteArray
        {
            get { return Serializer.CreateSerializer(typeof(WrappedValue<T>)).Serialize(this); }
        }
    }
}
