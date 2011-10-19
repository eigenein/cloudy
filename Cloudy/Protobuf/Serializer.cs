using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cloudy.Protobuf.Attributes;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf
{
    /// <summary>
    /// Represents a serializer.
    /// </summary>
    public class Serializer
    {
        private readonly Dictionary<int, PropertyInfo> tagToPropertyCache;

        private readonly Type type;

        protected Serializer(Type type, Dictionary<int, PropertyInfo> tagToPropertyCache)
        {
            this.tagToPropertyCache = tagToPropertyCache;
            this.type = type;
        }

        /// <summary>
        /// Gets or sets the extensions for this serializer.
        /// </summary>
        public Dictionary<Type, IValueSerializer> Extensions { get; set; }

        public static Serializer CreateSerializer(Type type)
        {
            MemberInfo typeInfo = type;
            if (typeInfo.GetCustomAttributes(typeof(Attributes.SerializableAttribute),
                false).Length == 0)
            {
                throw new ArgumentException("Non-serializable class.");
            }
            return new Serializer(type, CreateTagToPropertyCache(type));
        }

        private static Dictionary<int, PropertyInfo> CreateTagToPropertyCache(Type type)
        {
            Dictionary<int, PropertyInfo> tagToPropertyCache =
                new Dictionary<int, PropertyInfo>();
            foreach (PropertyInfo property in type.GetProperties())
            {
                foreach (TagAttribute tagAttribute in property.GetCustomAttributes(
                    typeof(TagAttribute), false).Cast<TagAttribute>())
                {
                    tagToPropertyCache[tagAttribute.Tag] = property;
                }
            }
            return tagToPropertyCache;
        }

        /// <summary>
        /// Serializes the value into the stream.
        /// </summary>
        public void Serialize(object value, Stream stream)
        {
            if (value.GetType() == type)
            {
                throw new ArgumentException(String.Format(
                    "{0} type expected, but was {1}", type, value.GetType()));
            }
            // TODO
        }

        /// <summary>
        /// Serializes the value into an array of bytes.
        /// </summary>
        public byte[] Serialize(object value)
        {
            MemoryStream stream = new MemoryStream();
            Serialize(value, stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Deserializes a value from the stream.
        /// </summary>
        /// <returns>Whether the deserialization has succeeded.</returns>
        public bool Deserialize(Stream stream, out object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deserializes a value from the array of bytes.
        /// </summary>
        /// <returns>Whether the deserialization has succeeded.</returns>
        public bool Deserialize(byte[] bytes, out object value)
        {
            return Deserialize(new MemoryStream(bytes), out value);
        }
    }
}
