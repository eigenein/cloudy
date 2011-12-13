using System;
using System.IO;
using Cloudy.Protobuf.Encoding;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.Serializers
{
    /// <summary>
    /// Serializes and deserializes <c>ulong</c> values.
    /// </summary>
    public class FixedUInt64Serializer : SerializerWithWireType
    {
        #region Overrides of AbstractSerializer<object>

        public override void Serialize(Stream stream, object o)
        {
            ProtobufWriter.WriteRawBytes(stream, BitConverter.GetBytes((ulong)o));
        }

        public override object Deserialize(Stream stream)
        {
            return BitConverter.ToUInt64(ProtobufReader.ReadRawBytes(stream, 8), 0);
        }

        #endregion

        #region Overrides of SerializerWithWireType

        public override WireType WireType
        {
            get { return WireType.Fixed64; }
        }

        #endregion
    }
}
