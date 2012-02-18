using System;

namespace Cloudy.Helpers
{
    /// <summary>
    /// Extends the <see cref="TimeSpan"/> class.
    /// </summary>
    public static class TimeSpanExtensions
    {
        /// <summary>
        /// Infinite <see cref="TimeSpan"/> constant.
        /// </summary>
        public static readonly TimeSpan Infinite = new TimeSpan(0, 0, 0, 0, -1);
    }
}
