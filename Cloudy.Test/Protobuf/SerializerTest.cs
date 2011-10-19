using System;
using Cloudy.Protobuf;
using Cloudy.Protobuf.Exceptions;
using Cloudy.Test.Protobuf.TestClasses;
using NUnit.Framework;

namespace Cloudy.Test.Protobuf
{
    [TestFixture]
    public class SerializerTest
    {
        [TestCase]
        public void TestBasic()
        {
            Serializer serializer = Serializer.CreateSerializer(typeof(A));
            object o = new A { B = 150 };
            AssertExtensions.AreEqual(new byte[] { 0x08, 0x96, 0x01 },
                serializer.Serialize(o));
        }

        [TestCase]
        public void TestMissingOptionalValue()
        {
            Serializer serializer = Serializer.CreateSerializer(typeof(B));
            object o = new B();
            AssertExtensions.AreEqual(new byte[] { },
                serializer.Serialize(o));
        }

        [TestCase]
        public void TestMissingRequiredValue()
        {
            Serializer serializer = Serializer.CreateSerializer(typeof(C));
            object o = new C();
            Assert.Throws<MissingValueException>(() => serializer.Serialize(o));
        }
    }
}
