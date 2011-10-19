using System;

namespace Cloudy.Protobuf.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public class ProtobufSerializableAttribute : Attribute
    {
        // Nothing here.
    }
}
