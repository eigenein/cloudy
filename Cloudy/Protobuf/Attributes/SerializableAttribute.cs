using System;

namespace Cloudy.Protobuf.Attributes
{
    /// <summary>
    /// Marks a class for serialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public class SerializableAttribute : Attribute
    {
        // Empty.
    }
}
