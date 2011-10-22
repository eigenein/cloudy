using System;
using System.IO;
using Cloudy.Protobuf.Encoding;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.Serializers
{
    public class GuidSerializer : WireTypedSerializer
    {
        #region Overrides of AbstractSerializer

        public override void Serialize(Stream stream, object o)
        {
            ProtobufWriter.WriteBytes(stream, ((Guid)o).ToByteArray());
        }

        public override object Deserialize(Stream stream)
        {
            return new Guid(ProtobufReader.ReadBytes(stream));
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
