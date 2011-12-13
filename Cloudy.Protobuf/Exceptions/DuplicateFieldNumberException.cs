using System;

namespace Cloudy.Protobuf.Exceptions
{
    /// <summary>
    /// Raised when the same field number is used for two different
    /// properties.
    /// </summary>
    [Serializable]
    public class DuplicateFieldNumberException : Exception
    {
        public DuplicateFieldNumberException(uint fieldNumber)
            : base(String.Format("Duplicate field number: {0}", fieldNumber))
        {
            // Do nothing.
        }
    }
}
