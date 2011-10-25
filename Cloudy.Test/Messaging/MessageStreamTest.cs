using System;
using System.IO;
using Cloudy.Messaging;
using Cloudy.Test.Protobuf.TestClasses;
using NUnit.Framework;

namespace Cloudy.Test.Messaging
{
    [TestFixture]
    public class MessageStreamTest
    {
        [Test]
        public void TestWrite()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                MessageStream messageStream = new MessageStream(stream);
                foreach (object message in 
                    new object[] { new A { B = 1 }, new A { B = 2 } })
                {
                    messageStream.Write(message);
                }
                AssertExtensions.AreEqual(
                    new byte[] { 0x02, 0x08, 0x01, 0x02, 0x08, 0x02 }, 
                    stream.ToArray());
            }
        }
    }
}
