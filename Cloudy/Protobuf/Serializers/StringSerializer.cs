using System;
using System.IO;
using Cloudy.Protobuf.Encoding;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.Serializers
{
    public class StringSerializer : WireTypedSerializer
    {
        #region Overrides of AbstractSerializer<object>

        public override void Serialize(Stream stream, object o)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes((string)o);
            ProtobufWriter.WriteBytes(stream, bytes);
        }

        public override object Deserialize(Stream stream)
        {
            return System.Text.Encoding.UTF8.GetString(
                ProtobufReader.ReadBytes(stream));
        }

        #endregion

        #region Overrides of WireTypedSerializer

        public override WireType WireType
        {
            get { return WireType.LengthDelimited; }
        }

        #endregion
    }
}
