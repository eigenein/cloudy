using System;
using System.IO;
using Cloudy.Protobuf.Encoding;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.Serializers
{
    public class UnsignedVarintSerializer : WireTypedSerializer
    {
        private readonly Func<ulong, object> convertFunction;

        public UnsignedVarintSerializer()
        {
            this.convertFunction = value => value;
        }

        public UnsignedVarintSerializer(Func<ulong, object> convertFunction)
        {
            this.convertFunction = convertFunction;
        }

        #region Overrides of AbstractSerializer<object>

        public override void Serialize(Stream stream, object o)
        {
            ProtobufWriter.WriteUnsignedVarint(stream, Convert.ToUInt64(o));
        }

        public override object Deserialize(Stream stream)
        {
            return convertFunction(ProtobufReader.ReadUnsignedVarint(stream));
        }

        #endregion

        #region Overrides of WireTypedSerializer

        public override WireType WireType
        {
            get { return WireType.Varint; }
        }

        #endregion
    }
}
