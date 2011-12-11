using System;
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
    }
}
