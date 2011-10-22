using System;
using System.IO;
using Cloudy.Protobuf.Encoding;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.Serializers
{
    public class FixedInt64Serializer : WireTypedSerializer
    {
        #region Overrides of AbstractSerializer<object>

        public override void Serialize(Stream stream, object o)
        {
            ProtobufWriter.WriteRawBytes(stream, BitConverter.GetBytes((long)o));
        }

        public override object Deserialize(Stream stream)
        {
            return BitConverter.ToInt64(ProtobufReader.ReadRawBytes(stream, 8), 0);
        }

        #endregion

        #region Overrides of WireTypedSerializer

        public override WireType WireType
        {
            get { return WireType.Fixed64; }
        }

        #endregion
    }
}
