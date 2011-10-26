using System;
using System.Net;
using System.Net.Sockets;
using Cloudy.Helpers;
using NUnit.Framework;

namespace Cloudy.Test.Helpers
{
    [TestFixture]
    public class UdpStreamTest
    {
        private const int Port = 1234;

        [Test]
        public void TestReadWrite()
        {
            UdpStream stream1 = new UdpStream(new UdpClient(
                new IPEndPoint(IPAddress.Any, Port)));
            UdpStream stream2 = new UdpStream(new UdpClient());
            stream2.Client.Connect("localhost", Port);
            byte[] buffer = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            stream2.Write(buffer, 0, buffer.Length);
            foreach (byte b in buffer)
            {
                Assert.AreEqual(b, (byte)stream1.ReadByte());
            }
        }
    }
}
