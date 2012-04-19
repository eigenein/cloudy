using System;
using System.IO;
using Cloudy.Helpers;
using Cloudy.Messaging;
using Cloudy.Messaging.Raw;
using Cloudy.Test.Protobuf.TestClasses;
using NUnit.Framework;

namespace Cloudy.Test.Messaging
{
    [TestFixture]
    public class CommunicatorTest
    {
        [Test]
        public void TestWrite()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                Communicator communicator = new Communicator(
                    new StreamSimpleCommunicator<object>(stream, null));
                foreach (object message in 
                    new object[] { new A { UIntValue = 1 }, new A { UIntValue = 2 } })
                {
                    communicator.Send(message);
                }
                AssertExtensions.AreEqual(
                    new byte[] { 0x02, 0x08, 0x01, 0x02, 0x08, 0x02 }, 
                    stream.ToArray());
            }
        }
    }
}
