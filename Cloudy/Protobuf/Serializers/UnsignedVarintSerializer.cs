using System;
using System.IO;
using Cloudy.Protobuf.Encoding;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.Serializers
{
    /// <summary>
    /// Used to serialize and deserialize integer values into and from
    /// unsigned Varint's.
    /// </summary>
    public class UnsignedVarintSerializer : SerializerWithWireType
    {
        private readonly Func<ulong, object> convertFunction;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public UnsignedVarintSerializer()
        {
            this.convertFunction = value => value;
        }

        /// <summary>
        /// Initalizes a new instance.
        /// </summary>
        /// <param name="convertFunction">
        /// The function that converts a deserializes <c>ulong</c> value
        /// to a value of the target type.
        /// </param>
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

        #region Overrides of SerializerWithWireType

        public override WireType WireType
        {
            get { return WireType.Varint; }
        }

        #endregion
    }
}
