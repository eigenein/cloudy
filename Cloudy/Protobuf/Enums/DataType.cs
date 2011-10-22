using System;

namespace Cloudy.Protobuf.Enums
{
    /// <summary>
    /// Represents the way in what a value should be encoded to the Protobuf format.
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// The default way for the specified .NET type will be used.
        /// </summary>
        Default = 0,

        Bool,
        Bytes,
        FixedInt32,
        FixedUInt32,
        FixedInt64,
        FixedUInt64,
        SignedVarint,
        UnsignedVarint,
        String,
        Guid
    }
}
