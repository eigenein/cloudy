using System;
using System.IO;
using Cloudy.Protobuf.Encoding;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.Serializers
{
    public class SingleSerializer : SerializerWithWireType
    {
        #region Overrides of AbstractSerializer

        public override void Serialize(Stream stream, object o)
        {
            ProtobufWriter.WriteRawBytes(stream, BitConverter.GetBytes((float)o));
        }

        public override object Deserialize(Stream stream)
        {
            return BitConverter.ToSingle(ProtobufReader.ReadRawBytes(stream, 4), 0);
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
