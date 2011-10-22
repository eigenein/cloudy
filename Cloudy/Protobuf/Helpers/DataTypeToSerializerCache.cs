using System;
using System.Collections.Generic;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Interfaces;
using Cloudy.Protobuf.Serializers;

namespace Cloudy.Protobuf.Helpers
{
    /// <summary>
    /// Contains default serializers for the specific serialization ways.
    /// </summary>
    internal static class DataTypeToSerializerCache
    {
        private static readonly Dictionary<DataType, SerializerWithWireType> Cache =
            new Dictionary<DataType, SerializerWithWireType>()
            {
                {DataType.Bool, new BoolSerializer()},
                {DataType.Bytes, new BytesSerializer()},
                {DataType.FixedInt32, new FixedInt32Serializer()},
                {DataType.FixedInt64, new FixedInt64Serializer()},
                {DataType.FixedUInt32, new FixedUInt32Serializer()},
                {DataType.FixedUInt64, new FixedUInt64Serializer()},
                {DataType.SignedVarint, new SignedVarintSerializer()},
                {DataType.String, new StringSerializer()},
                {DataType.UnsignedVarint, new UnsignedVarintSerializer()},
                {DataType.Guid, new GuidSerializer()}
            };

        public static bool TryGetSerializer(DataType dataType, out SerializerWithWireType serializer)
        {
            return Cache.TryGetValue(dataType, out serializer);
        }
    }
}
