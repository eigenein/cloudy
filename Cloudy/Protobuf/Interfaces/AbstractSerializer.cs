using System;
using System.IO;

namespace Cloudy.Protobuf.Interfaces
{
    /// <summary>
    /// Represents the abstract serializer.
    /// </summary>
    public abstract class AbstractSerializer
    {
        public abstract void Serialize(Stream stream, object o);

        /// <summary>
        /// Serializes the object into bytes.
        /// </summary>
        public byte[] Serialize(object o)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Serialize(stream, o);
                return stream.ToArray();
            }
        }

        public abstract object Deserialize(Stream stream);

        /// <summary>
        /// Deserializes an object from the byte array.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public object Deserialize(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                return Deserialize(stream);
            }
        }

        /// <summary>
        /// Returns whether the value should be silently skipped during serializing.
        /// </summary>
        public virtual bool ShouldBeSkipped(object value)
        {
            return false;
        }
    }
}
