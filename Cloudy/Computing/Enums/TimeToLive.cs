using System;

namespace Cloudy.Computing.Enums
{
    /// <summary>
    /// Represents a TTL for a value in a remote storage.
    /// </summary>
    public enum TimeToLive
    {
        /// <summary>
        /// A value should be cached forever.
        /// </summary>
        Forever,

        /// <summary>
        /// A value should be cached during the current job execution.
        /// </summary>
        JobSpecific,

        /// <summary>
        /// A value must not be cached.
        /// </summary>
        Flash
    }
}