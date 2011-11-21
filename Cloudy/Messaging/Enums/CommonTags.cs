using System;

namespace Cloudy.Messaging.Enums
{
    /// <summary>
    /// Defines the well-known tags (negative).
    /// </summary>
    public static class CommonTags
    {
        /// <summary>
        /// The delivery notification tag.
        /// </summary>
        public const int Receipt = -1;

        /// <summary>
        /// The ping tag. Ping DTO's are not buffered on the destination
        /// and used only to test connection.
        /// </summary>
        public const int Ping = -2;

        /// <summary>
        /// Indicates a request for joining to a network.
        /// </summary>
        public const int JoinRequest = -3;

        /// <summary>
        /// Indicates a master response to joining to a network.
        /// </summary>
        public const int JoinResponse = -4;

        /// <summary>
        /// Thread allocation on a slave.
        /// </summary>
        public const int AllocateThread = -5;

        /// <summary>
        /// A shutdown notification.
        /// </summary>
        public const int Bye = -6;

        /// <summary>
        /// A neighbor information.
        /// </summary>
        public const int Neighbor = -7;
    }
}
