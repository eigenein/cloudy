using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Test.Protobuf.TestClasses
{
    [ProtobufSerializable]
    public class A
    {
        /// <summary>
        /// Initializes the default values.
        /// </summary>
        public A()
        {
            UIntValue = 666;
        }

        [ProtobufField(1)]
        public uint UIntValue { get; set; }
    }
}
