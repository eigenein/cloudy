using System;

namespace Cloudy.Protobuf.Attributes
{
    /// <summary>
    /// Marks the class as serializable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public class ProtobufSerializableAttribute : Attribute
    {
        // Nothing here.
    }
}
