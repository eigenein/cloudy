using System;
using System.IO;
using Cloudy.Protobuf.Encoding;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Helpers;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.Serializers
{
    public class EmbeddedMessageSerializer : WireTypedSerializer
    {
        private readonly Serializer serializer;

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
            ulong length = ProtobufReader.ReadUnsignedVarint(stream);
            return serializer.Deserialize(new StreamSegment(stream, (long)length));
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
