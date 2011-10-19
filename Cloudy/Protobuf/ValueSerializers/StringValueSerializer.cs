using System;
using System.IO;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Helpers;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.ValueSerializers
{
    public class StringValueSerializer : IValueSerializer
    {
        #region Implementation of IValueSerializer

        public WireType WireType
        {
            get { return WireType.LengthDelimited; }
        }

        public void Serialize(object value, Stream stream)
        {
            ProtobufWriter.WriteString((string)value, stream);
        }

        public bool Deserialize(Stream stream, out object value)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
