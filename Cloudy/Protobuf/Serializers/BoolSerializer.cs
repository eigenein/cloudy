using System;
using System.IO;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.Serializers
{
    public class BoolSerializer : SerializerWithWireType
    {
        #region Implementation of ISerializer<object>

        public override void Serialize(Stream stream, object o)
        {
            stream.WriteByte((bool)o ? (byte)1 : (byte)0);
        }

        public override object Deserialize(Stream stream)
        {
            int b = stream.ReadByte();
            if (b == -1)
            {
                throw new InvalidDataException("Unexpected end of stream.");
            }
            return b == 1;
        }

        #endregion

        public override WireType WireType
        {
            get { return WireType.Varint; }
        }
    }
}
