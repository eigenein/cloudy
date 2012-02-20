using System;
using System.IO;
using Cloudy.Protobuf.Encoding;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.Serializers
{
    public class DoubleSerializer : SerializerWithWireType
    {
        #region Overrides of AbstractSerializer

        public override void Serialize(Stream stream, object o)
        {
            ProtobufWriter.WriteRawBytes(stream, BitConverter.GetBytes((double)o));
        }

        public override object Deserialize(Stream stream)
        {
            return BitConverter.ToDouble(ProtobufReader.ReadRawBytes(stream, 8), 0);
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
