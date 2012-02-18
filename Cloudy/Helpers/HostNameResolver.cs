using System;
using System.Net;
using System.Net.Sockets;

namespace Cloudy.Helpers
{
    /// <summary>
    /// Helps to resolve a hostname to an IP address.
    /// </summary>
    public static class HostNameResolver
    {
        /// <summary>
        /// Resolves the hostname to IP address.
        /// </summary>
        /// <param name="hostName">The destination hostname.</param>
        /// <returns>The IP address of the host.</returns>
        /// <exception cref="SocketException">
        /// No such host is known.
        /// </exception>
        public static IPAddress Resolve(string hostName)
        {
            IPAddress[] addresses = Dns.GetHostAddresses(hostName);
            if (addresses.Length == 0)
            {
                throw new SocketException((int)SocketError.HostNotFound);
            }
            return addresses[0];
        }
    }
}
