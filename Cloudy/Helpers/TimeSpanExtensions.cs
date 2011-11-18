using System;

namespace Cloudy.Helpers
{
    public static class TimeSpanExtensions
    {
        public static readonly TimeSpan Infinite = new TimeSpan(0, 0, 0, 0, -1);
    }
}
