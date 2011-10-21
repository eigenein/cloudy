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
        [Test]
        public void TestSerializeBasic()
        {
            Serializer serializer = Serializer.CreateSerializer(typeof(A));
            object o = new A { B = 150 };
            AssertExtensions.AreEqual(new byte[] { 0x08, 0x96, 0x01 },
                serializer.Serialize(o));
        }

        [Test]
        public void TestSerializeMissingOptionalValue()
        {
            Serializer serializer = Serializer.CreateSerializer(typeof(B));
            object o = new B();
            AssertExtensions.AreEqual(new byte[] { },
                serializer.Serialize(o));
        }

        [Test]
        public void TestSerializeMissingRequiredValue()
        {
            Serializer serializer = Serializer.CreateSerializer(typeof(C));
            object o = new C();
            Assert.Throws<MissingValueException>(() => serializer.Serialize(o));
        }

        [Test]
        public void TestSerializeRepeatedValue()
        {
            Serializer serializer = Serializer.CreateSerializer(typeof(D));
            object o = new D { List = new uint[] { 1, 2, 3 } };
            AssertExtensions.AreEqual(new byte[] { 0x08, 0x01, 0x08, 0x02, 0x08, 0x03 },
                serializer.Serialize(o));
        }

        [Test]
        public void TestSerializePackedRepeatedValue()
        {
            Serializer serializer = Serializer.CreateSerializer(typeof(E));
            object o = new E { List = new uint[] { 3, 270, 86942 } };
            AssertExtensions.AreEqual(new byte[] {
                0x22, 0x06, 0x03, 0x8E, 0x02, 0x9E, 0xA7, 0x05 },
                serializer.Serialize(o));
        }

        [Test]
        public void TestDeserializeMissingOptionalValue()
        {
            Serializer serializer = Serializer.CreateSerializer(typeof(A));
            A a = (A)serializer.Deserialize(new byte[] { });
            Assert.AreEqual(666, a.B);
        }

        [Test]
        public void TestDeserializeMissingRequiredValue()
        {
            Serializer serializer = Serializer.CreateSerializer(typeof(C));
            Assert.Throws<MissingValueException>(() => serializer.Deserialize(
                new byte[] { }));
        }

        [Test]
        public void TestDeserializeRepeatedNonRepeatedField()
        {
            Serializer serializer = Serializer.CreateSerializer(typeof(A));
            A a = (A)serializer.Deserialize(new byte[] { 0x08, 0x01, 0x08, 0x02, 0x08, 0x03 });
            Assert.AreEqual(a.B, 3u);
        }

        [Test]
        public void TestDeserializeRepeatedField()
        {
            Serializer serializer = Serializer.CreateSerializer(typeof(D));
            D d = (D)serializer.Deserialize(new byte[] { 0x08, 0x01, 0x08, 0x02, 0x08, 0x03 });
            AssertExtensions.AreEqual(d.List, new uint[] { 1, 2, 3});
        }

        [Test]
        public void TestDeserializePackedRepeatedField()
        {
            Serializer serializer = Serializer.CreateSerializer(typeof(E));
            E d = (E)serializer.Deserialize(new byte[] { 0x22, 0x06, 0x03, 
                0x8E, 0x02, 0x9E, 0xA7, 0x05 });
            AssertExtensions.AreEqual(d.List, new uint[] { 3, 270, 86942 });
        }

        [Test]
        public void TestSerializeEmbeddedMessage()
        {
            F f = new F() { Message = new A() { B = 150 } };
            Serializer serializer = Serializer.CreateSerializer(typeof(F));
            AssertExtensions.AreEqual(new byte[] { 0x1a, 0x03, 0x08, 0x96, 0x01 },
                serializer.Serialize(f));
        }

        [Test]
        public void TestEmbeddedMessageBoundaries()
        {
            G g = new G() { A1 = 1, A2 = new A() { B = 2 }, A3 = 3 };
            Serializer serializer = Serializer.CreateSerializer(typeof(G));
            g = (G)serializer.Deserialize(serializer.Serialize(g));
            Assert.AreEqual(g.A1, 1);
            Assert.AreEqual(g.A3, 3);
            Assert.NotNull(g.A2);
            Assert.AreEqual(g.A2.B, 2);
        }

        [Test]
        public void TestDeserializeEmbeddedMessage()
        {
            Serializer serializer = Serializer.CreateSerializer(typeof(F));
            F f = (F)serializer.Deserialize(new byte[] { 0x1a, 0x03, 0x08, 0x96, 0x01 });
            Assert.IsNotNull(f.Message);
            Assert.AreEqual(f.Message.B, 150);
        }
    }
}
