using System;
using System.IO;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Exceptions;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.Serializers
{
    /// <summary>
    /// The proxy serializer - used to check a value for the <c>null</c>
    /// value.
    /// </summary>
    public class NullProxySerializer : SerializerWithWireType
    {
        private readonly SerializerWithWireType serializer;

        private readonly bool allowNull;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="serializer">The underlying serializer.</param>
        /// <param name="allowNull">
        /// Defines whether <c>null</c> is allowed
        /// as a field value.
        /// </param>
        public NullProxySerializer(SerializerWithWireType serializer,
            bool allowNull)
        {
            this.serializer = serializer;
            this.allowNull = allowNull;
        }

        #region Overrides of AbstractSerializer

        public override void Serialize(Stream stream, object o)
        {
            if (o != null)
            {
                serializer.Serialize(stream, o);
            }
            else if (!allowNull)
            {
                throw new MissingValueException("A field is not allowed to be null.");
            }
        }

        public override object Deserialize(Stream stream)
        {
            return serializer.Deserialize(stream);
        }

        public override bool ShouldBeSkipped(object value)
        {
            return serializer.ShouldBeSkipped(value) || (value == null && allowNull);
        }

        #endregion

        #region Overrides of SerializerWithWireType

        public override WireType WireType
        {
            get { return serializer.WireType; }
        }

        #endregion
    }
}
