using System;
using Cloudy.Protobuf.Serializers;
using NUnit.Framework;

namespace Cloudy.Test.Protobuf.Serializers
{
    [TestFixture]
    public class UnsignedVarintSerializerTest
    {
        private static readonly UnsignedVarintSerializer Serializer =
            new UnsignedVarintSerializer();

        [TestCase(0ul, new byte[] { 0x00 })]
        [TestCase(3ul, new byte[] { 0x03 })]
        [TestCase(270ul, new byte[] { 0x8E, 0x02 })]
        [TestCase(86942ul, new byte[] { 0x9E, 0xA7, 0x05 })]
        public void TestSerialize(ulong value, byte[] expected)
        {
            AssertExtensions.AreEqual(expected, Serializer.Serialize(value));
        }

        [TestCase(0ul, new byte[] { 0x00 })]
        [TestCase(3ul, new byte[] { 0x03 })]
        [TestCase(270ul, new byte[] { 0x8E, 0x02 })]
        [TestCase(86942ul, new byte[] { 0x9E, 0xA7, 0x05 })]
        public void TestDeserialize(ulong expected, byte[] value)
        {
            Assert.AreEqual(expected, Serializer.Deserialize(value));
        }
    }
}
