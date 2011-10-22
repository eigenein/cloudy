using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cloudy.Protobuf.Encoding;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.Serializers
{
    public class RepeatedSerializer : SerializerWithWireType
    {
        private readonly SerializerWithWireType serializer;

        private readonly uint fieldNumber;

        public RepeatedSerializer(uint fieldNumber, SerializerWithWireType serializer)
        {
            this.fieldNumber = fieldNumber;
            this.serializer = serializer;
        }

        #region Overrides of AbstractSerializer

        public override void Serialize(Stream stream, object o)
        {
            IEnumerable<object> elements = ((ICollection)o).Cast<object>();
            bool first = true;
            foreach (object element in elements)
            {
                if (!first)
                {
                    ProtobufWriter.WriteKey(stream, fieldNumber, WireType);
                }
                else
                {
                    first = false;
                }
                serializer.Serialize(stream, element);
            }
        }

        public override object Deserialize(Stream stream)
        {
            List<object> list = new List<object>();
            list.Add(serializer.Deserialize(stream));
            return list;
        }

        #endregion

        #region Overrides of WireTypedSerializer

        public override WireType WireType
        {
            get { return serializer.WireType; }
        }

        #endregion
    }
}
