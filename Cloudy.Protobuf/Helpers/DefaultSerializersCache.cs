using System;
using System.Collections.Generic;
using Cloudy.Protobuf.Interfaces;
using Cloudy.Protobuf.Serializers;

namespace Cloudy.Protobuf.Helpers
{
    /// <summary>
    /// Contains default serializers for the .NET types.
    /// </summary>
    internal static class DefaultSerializersCache
    {
        private static readonly Dictionary<Type, SerializerWithWireType> Cache =
            new Dictionary<Type, SerializerWithWireType>()
            {
                {typeof(bool), new BoolSerializer()},
                {typeof(int), new SignedVarintSerializer(value => Convert.ToInt32(value))},
                {typeof(long), new SignedVarintSerializer()},
                {typeof(uint), new UnsignedVarintSerializer(value => Convert.ToUInt32(value))},
                {typeof(ulong), new UnsignedVarintSerializer()},
                {typeof(string), new StringSerializer()},
                {typeof(byte[]), new BytesSerializer()},
                {typeof(Guid), new GuidSerializer()}
            };

        public static bool TryGetSerializer(Type type,
            out SerializerWithWireType serializer)
        {
            return Cache.TryGetValue(type, out serializer);
        }
    }
}
