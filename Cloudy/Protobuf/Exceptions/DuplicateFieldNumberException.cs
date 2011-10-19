using System;

namespace Cloudy.Protobuf.Exceptions
{
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
