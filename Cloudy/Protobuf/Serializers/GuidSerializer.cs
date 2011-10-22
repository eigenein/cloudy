using System;
using System.IO;
using Cloudy.Protobuf.Encoding;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.Serializers
{
    /// <summary>
    /// Serializes and deserializes GUID's.
    /// </summary>
    public class GuidSerializer : SerializerWithWireType
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

        #region Overrides of SerializerWithWireType

        public override WireType WireType
        {
            get { return WireType.LengthDelimited; }
        }

        #endregion
    }
}
