using System;
using Cloudy.Protobuf.Enums;

namespace Cloudy.Protobuf.Interfaces
{
    /// <summary>
    /// Represents an abstract serializer with the specified wire type.
    /// </summary>
    public abstract class SerializerWithWireType : AbstractSerializer
    {
        public abstract WireType WireType { get; }
    }
}
