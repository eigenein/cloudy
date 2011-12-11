using System;
using System.Collections.Generic;
using System.Net;

namespace Cloudy.Helpers
{
    internal static class IPEndPointExtensions
    {
        public static IPEndPoint Clone(this IPEndPoint endPoint)
        {
            return new IPEndPoint(endPoint.Address, endPoint.Port);
        }

        public static IEnumerable<IPEndPoint> GetPortScanEndPoints(
            this IPEndPoint initialEndPoint, int maxOffset)
        {
            IPEndPoint current = initialEndPoint.Clone();
            int nextPortOffset = 0;
            while (Math.Abs(nextPortOffset) <= 2 * maxOffset)
            {
                current.Port += nextPortOffset;
                yield return current;
                nextPortOffset = -nextPortOffset + (nextPortOffset >= 0 ? -1 : 1);
            }
        }
    }
}
