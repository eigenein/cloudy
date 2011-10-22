using System;
using Cloudy.Protobuf.Enums;

namespace Cloudy.Protobuf.Interfaces
{
    public abstract class SerializerWithWireType : AbstractSerializer
    {
        public abstract WireType WireType { get; }
    }
}
