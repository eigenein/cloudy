using System;
using System.Collections.Generic;
using Cloudy.Protobuf.Interfaces;
using Cloudy.Protobuf.ValueSerializers;

namespace Cloudy.Protobuf.Caches
{
    internal static class TypeToSerializerCache
    {
        private static readonly Dictionary<Type, IValueSerializer> Cache =
            new Dictionary<Type, IValueSerializer>()
            {
                {typeof(String), new StringValueSerializer()}
            };

        public static bool TryGetSerializer(Type type, out IValueSerializer serializer)
        {
            return Cache.TryGetValue(type, out serializer);
        }
    }
}
