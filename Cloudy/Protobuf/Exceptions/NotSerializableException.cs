using System;

namespace Cloudy.Protobuf.Exceptions
{
    [Serializable]
    public class NotSerializableException : Exception
    {
        public NotSerializableException(Type type)
            : base(String.Format("The type is not marked for serialization: {0}", type))
        {
            // Do nothing.
        }
    }
}
