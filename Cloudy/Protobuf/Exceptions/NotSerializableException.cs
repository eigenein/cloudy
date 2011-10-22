using System;

namespace Cloudy.Protobuf.Exceptions
{
    /// <summary>
    /// Raised when a class is not marked for serialization.
    /// </summary>
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
