using System;
using System.IO;
using Cloudy.Protobuf.Encoding;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.Serializers
{
    /// <summary>
    /// Serializes and deserializes <c>int</c> values.
    /// </summary>
    public class FixedInt32Serializer : SerializerWithWireType
    {
        #region Overrides of AbstractSerializer<object>

        public override void Serialize(Stream stream, object o)
        {
            ProtobufWriter.WriteRawBytes(stream, BitConverter.GetBytes((int)o));
        }

        public override object Deserialize(Stream stream)
        {
            return BitConverter.ToInt32(ProtobufReader.ReadRawBytes(stream, 4), 0);
        }

        #endregion

        #region Overrides of SerializerWithWireType

        public override WireType WireType
        {
            get { return WireType.Fixed32; }
        }

        #endregion
    }
}
