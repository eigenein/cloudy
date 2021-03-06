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
    /// <summary>
    /// The proxy serializer - serializes and deserializes packed repeated 
    /// fields.
    /// </summary>
    public class PackedRepeatedSerializer : SerializerWithWireType
    {
        private readonly SerializerWithWireType serializer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="serializer">The underlying serializer.</param>
        public PackedRepeatedSerializer(SerializerWithWireType serializer)
        {
            this.serializer = serializer;
        }

        #region Overrides of AbstractSerializer

        public override void Serialize(Stream stream, object o)
        {
            IEnumerable<object> elements = ((ICollection)o).Cast<object>();
            using (MemoryStream internalStream = new MemoryStream())
            {
                foreach (object element in elements)
                {
                    serializer.Serialize(internalStream, element);
                }
                ProtobufWriter.WriteBytes(stream, internalStream.ToArray());
            }
        }

        public override object Deserialize(Stream stream)
        {
            List<object> result = new List<object>();
            using (MemoryStream internalStream = new MemoryStream(
                ProtobufReader.ReadBytes(stream)))
            {
                while (internalStream.Position < internalStream.Length)
                {
                    result.Add(serializer.Deserialize(internalStream));
                }
            }
            return result;
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
