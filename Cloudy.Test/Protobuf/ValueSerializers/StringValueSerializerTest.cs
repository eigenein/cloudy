using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cloudy.Protobuf.ValueSerializers;
using NUnit.Framework;

namespace Cloudy.Test.Protobuf.ValueSerializers
{
    [TestFixture]
    internal class StringValueSerializerTest
    {
        private static readonly StringValueSerializer Serializer =
            new StringValueSerializer();

        [TestCase("Привет", new byte[] { 0x0c, 0xd0, 0x9f, 0xd1, 0x80, 0xd0,
            0xb8, 0xd0, 0xb2, 0xd0, 0xb5, 0xd1, 0x82 })]
        public void TestSerialize(string value, IEnumerable<byte> expected)
        {
            MemoryStream stream = new MemoryStream();
            Serializer.Serialize(value, stream);
            Assert.IsTrue(expected.SequenceEqual(stream.ToArray()));
        }
    }
}
