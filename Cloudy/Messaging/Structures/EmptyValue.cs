using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Messaging.Structures
{
    /// <summary>
    /// Empty value class when a message should not have any value attached.
    /// </summary>
    [ProtobufSerializable]
    public class EmptyValue
    {
        private static readonly EmptyValue instance = new EmptyValue();

        public static EmptyValue Instance
        {
            get { return instance; }
        }
    }
}
