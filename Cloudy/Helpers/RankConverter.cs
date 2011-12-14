using System;
using Cloudy.Computing.Topologies.Interfaces;
using Cloudy.Protobuf;

namespace Cloudy.Helpers
{
    public class RankConverter<TRank>
        where TRank : IRank
    {
        private static readonly Serializer Serializer =
            Serializer.CreateSerializer(typeof(TRank));

        public static byte[] Convert(TRank value)
        {
            return Serializer.Serialize(value);
        }

        public static TRank Convert(byte[] value)
        {
            return (TRank)Serializer.Deserialize(value);
        }
    }
}
