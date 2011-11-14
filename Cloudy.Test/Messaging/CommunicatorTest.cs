﻿using System;
using System.IO;
using Cloudy.Helpers;
using Cloudy.Messaging;
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
                    new StreamRawCommunicator<object>(stream, null));
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

        [Test]
        public void TestWriteReadDto()
        {
            uint[] values = new uint[] { 1, 29, 34 };
            int[] tags = new int[] { 78, 10, 67 };
            using (MemoryStream stream = new MemoryStream())
            {
                using (Communicator communicator = new Communicator(
                    new StreamRawCommunicator<object>(stream, null)))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        communicator.SendTagged(tags[i], new A { UIntValue = values[i]});
                    }
                    stream.Position = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        int? tag;
                        uint value = communicator.ReceiveTagged(out tag).Cast<A>().UIntValue;
                        Assert.AreEqual(tags[i], tag);
                        Assert.AreEqual(values[i], value);
                    }
                }
            }
        }
    }
}