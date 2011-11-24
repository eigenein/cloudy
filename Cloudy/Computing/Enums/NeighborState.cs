using System;

namespace Cloudy.Computing.Enums
{
    /// <summary>
    /// Used when establishing an interconnection.
    /// </summary>
    public enum NeighborState
    {
        /// <summary>
        /// Initial state.
        /// </summary>
        Unknown,

        /// <summary>
        /// The first ping is sent.
        /// </summary>
        PingSent,

        /// <summary>
        /// Interconnection is established.
        /// </summary>
        Ok,

        /// <summary>
        /// The second ping is sent.
        /// </summary>
        PingSent2,

        /// <summary>
        /// Interconnection is not established.
        /// </summary>
        Failed
    }
}
