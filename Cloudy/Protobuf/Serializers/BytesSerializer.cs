﻿using System;
using System.IO;
using Cloudy.Protobuf.Encoding;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.Serializers
{
    public class BytesSerializer : WireTypedSerializer
    {
        #region Overrides of AbstractSerializer<object>

        public override void Serialize(Stream stream, object o)
        {
            ProtobufWriter.WriteBytes(stream, (byte[])o);
        }

        public override object Deserialize(Stream stream)
        {
            return ProtobufReader.ReadBytes(stream);
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
