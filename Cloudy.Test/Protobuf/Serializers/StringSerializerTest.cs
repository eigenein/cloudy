using System;
using Cloudy.Protobuf.Serializers;
using NUnit.Framework;

namespace Cloudy.Test.Protobuf.Serializers
{
    [TestFixture]
    public class StringSerializerTest
    {
        private static readonly StringSerializer Serializer =
            new StringSerializer();

        [TestCase("Привет", new byte[] { 0x0c, 0xd0, 0x9f, 0xd1, 0x80, 0xd0,
            0xb8, 0xd0, 0xb2, 0xd0, 0xb5, 0xd1,0x82 })]
        public void TestSerialize(string value, byte[] expected)
        {
            AssertExtensions.AreEqual(expected, Serializer.Serialize(value));
        }

        [TestCase("Привет", new byte[] { 0x0c, 0xd0, 0x9f, 0xd1, 0x80, 0xd0,
            0xb8, 0xd0, 0xb2, 0xd0, 0xb5, 0xd1,0x82 })]
        public void TestDeserialize(string expected, byte[] value)
        {
            Assert.AreEqual(expected, Serializer.Deserialize(value));
        }
    }
}
