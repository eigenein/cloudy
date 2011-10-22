using System;
using System.IO;
using Cloudy.Protobuf.Encoding;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.Serializers
{
    public class SignedVarintSerializer : SerializerWithWireType
    {
        private readonly Func<long, object> convertFunction;

        public SignedVarintSerializer()
        {
            this.convertFunction = value => value;
        }

        public SignedVarintSerializer(Func<long, object> convertFunction)
        {
            this.convertFunction = convertFunction;
        }

        #region Overrides of AbstractSerializer<object>

        public override void Serialize(Stream stream, object o)
        {
            ProtobufWriter.WriteSignedVarint(stream, Convert.ToInt64(o));
        }

        public override object Deserialize(Stream stream)
        {
            return convertFunction(ProtobufReader.ReadSignedVarint(stream));
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
