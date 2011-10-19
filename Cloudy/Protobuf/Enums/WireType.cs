using System;

namespace Cloudy.Protobuf.Enums
{
    /// <summary>
    /// Represents the Protobuf wire type.
    /// </summary>
    public enum WireType
    {
        Varint = 0,
        Fixed64 = 1,
        LengthDelimited = 2,
        Fixed32 = 5
    }
}
