using System;
using System.Net;
using System.Net.Sockets;
using Cloudy.Helpers;
using NUnit.Framework;

namespace Cloudy.Test.Helpers
{
    [TestFixture]
    public class HostNameResolverTest
    {
        [Test]
        public void TestLocalhost()
        {
            Assert.IsTrue(IPAddress.IsLoopback(HostNameResolver.Resolve("localhost")));
        }

        [Test]
        [ExpectedException(typeof(SocketException))]
        public void TestUnknown()
        {
            HostNameResolver.Resolve("trolo.lo");
        }
    }
}
