using System;
using System.IO;
using Cloudy.Protobuf.Encoding;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.Serializers
{
    /// <summary>
    /// Serializes and deserializes embedded messages.
    /// </summary>
    public class EmbeddedMessageSerializer : SerializerWithWireType
    {
        private readonly Serializer serializer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="serializer">The underlying message serializer.</param>
        public EmbeddedMessageSerializer(Serializer serializer)
        {
            this.serializer = serializer;
        }

        #region Overrides of Serializer

        public override void Serialize(Stream stream, object o)
        {
            ProtobufWriter.WriteBytes(stream, serializer.Serialize(o));
        }

        public override object Deserialize(Stream stream)
        {
            return serializer.Deserialize(ProtobufReader.ReadBytes(stream));
        }

        #endregion

        #region Overrides of SerializerWithWireType

        public override WireType WireType
        {
            get { return WireType.LengthDelimited; }
        }

        #endregion
    }
}
