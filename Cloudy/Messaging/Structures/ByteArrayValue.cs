using System;
using Cloudy.Protobuf;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Messaging.Structures
{
    [ProtobufSerializable]
    public class ByteArrayValue
    {
        [ProtobufField(1)]
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

        /// <summary>
        /// Serializes the value into an underlying byte array.
        /// </summary>
        public void Set<T>(T value)
        {
            Value = Serializer.CreateSerializer(typeof(T)).Serialize(value);
        }
    }
}
