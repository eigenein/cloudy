using System;

namespace Cloudy.Protobuf.Enums
{
    public enum DataType
    {
        Default = 0,
        Bool,
        Bytes,
        FixedInt32,
        FixedUInt32,
        FixedInt64,
        FixedUInt64,
        SignedVarint,
        UnsignedVarint,
        String
    }
}
