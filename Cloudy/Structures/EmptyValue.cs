using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Structures
{
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
