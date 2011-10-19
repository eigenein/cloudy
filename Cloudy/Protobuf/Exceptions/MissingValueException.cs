using System;

namespace Cloudy.Protobuf.Exceptions
{
    [Serializable]
    public class MissingValueException : Exception
    {
        public MissingValueException(string message)
            : base(message)
        {
            // Do nothing.
        }
    }
}
