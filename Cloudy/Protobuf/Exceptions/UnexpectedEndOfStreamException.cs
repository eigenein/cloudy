using System;

namespace Cloudy.Protobuf.Exceptions
{
    [Serializable]
    public class UnexpectedEndOfStreamException : Exception
    {
        public UnexpectedEndOfStreamException(string message)
            : base(message)
        {
            // Do nothing.
        }
    }
}
