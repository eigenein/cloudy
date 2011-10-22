using System;

namespace Cloudy.Protobuf.Exceptions
{
    /// <summary>
    /// Raised when a required field has no value set.
    /// </summary>
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
