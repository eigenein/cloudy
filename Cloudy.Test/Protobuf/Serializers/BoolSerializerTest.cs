using System;
using Cloudy.Protobuf.Serializers;
using NUnit.Framework;

namespace Cloudy.Test.Protobuf.Serializers
{
    [TestFixture]
    public class BoolSerializerTest
    {
        private static readonly BoolSerializer Serializer =
            new BoolSerializer();

        [TestCase(false, new byte[] { 0x00 })]
        [TestCase(true, new byte[] { 0x01 })]
        public void TestSerialize(bool value, byte[] expected)
        {
            AssertExtensions.AreEqual(expected, Serializer.Serialize(value));
        }

        [TestCase(false, new byte[] { 0x00 })]
        [TestCase(true, new byte[] { 0x01 })]
        public void TestDeserialize(bool expected, byte[] value)
        {
            Assert.AreEqual(expected, Serializer.Deserialize(value));
        }
    }
}
