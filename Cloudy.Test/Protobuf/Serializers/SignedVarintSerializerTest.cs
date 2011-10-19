using System;
using Cloudy.Protobuf.Serializers;
using NUnit.Framework;

namespace Cloudy.Test.Protobuf.Serializers
{
    [TestFixture]
    public class SignedVarintSerializerTest
    {
        private static readonly SignedVarintSerializer Serializer =
            new SignedVarintSerializer();

        [TestCase(0L, new byte[] { 0x00 })]
        [TestCase(-1L, new byte[] { 0x01 })]
        [TestCase(1L, new byte[] { 0x02 })]
        [TestCase(-2L, new byte[] { 0x03 })]
        public void TestSerialize(long value, byte[] expected)
        {
            AssertExtensions.AreEqual(expected, Serializer.Serialize(value));
        }

        [TestCase(0L, new byte[] { 0x00 })]
        [TestCase(-1L, new byte[] { 0x01 })]
        [TestCase(1L, new byte[] { 0x02 })]
        [TestCase(-2L, new byte[] { 0x03 })]
        public void TestDeserialize(long expected, byte[] value)
        {
            Assert.AreEqual(expected, Serializer.Deserialize(value));
        }
    }
}
