using System;
using System.IO;
using Cloudy.Protobuf.Serializers;
using NUnit.Framework;

namespace Cloudy.Test.Protobuf.Serializers
{
    [TestFixture]
    public class DoubleSerializerTest
    {
        [Test]
        public void Test()
        {
            double expected = 123.0;
            DoubleSerializer serializer = new DoubleSerializer();
            using (Stream stream = new MemoryStream())
            {
                serializer.Serialize(stream, expected);
                stream.Seek(0L, SeekOrigin.Begin);
                Assert.AreEqual(expected, (double)serializer.Deserialize(stream));
            }
        }
    }
}
