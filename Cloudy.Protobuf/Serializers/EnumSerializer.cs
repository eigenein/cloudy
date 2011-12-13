using System;
using System.IO;
using Cloudy.Protobuf.Encoding;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.Serializers
{
    /// <summary>
    /// Serializes and deserializes enums.
    /// </summary>
    public class EnumSerializer : SerializerWithWireType
    {
        private readonly Type expectedType;

        public EnumSerializer(Type expectedType)
        {
            this.expectedType = expectedType;
        }

        #region Overrides of AbstractSerializer

        public override void Serialize(Stream stream, object o)
        {
            ProtobufWriter.WriteSignedVarint(stream, (int)o);
        }

        public override object Deserialize(Stream stream)
        {
            return Enum.ToObject(expectedType, 
                (int)ProtobufReader.ReadSignedVarint(stream));
        }

        #endregion

        #region Overrides of SerializerWithWireType

        public override WireType WireType
        {
            get { return WireType.Varint; }
        }

        #endregion
    }
}
