using System;
using System.IO;
using Cloudy.Protobuf.Enums;

namespace Cloudy.Protobuf.Interfaces
{
    /// <summary>
    /// Represents a single value serializer.
    /// </summary>
    public interface IValueSerializer
    {
        WireType WireType { get; }

        void Serialize(object value, Stream stream);

        bool Deserialize(Stream stream, out object value);
    }
}
