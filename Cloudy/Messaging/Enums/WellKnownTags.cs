using System;

namespace Cloudy.Messaging.Enums
{
    /// <summary>
    /// Defines the well-known tags (negative).
    /// </summary>
    public static class WellKnownTags
    {
        /// <summary>
        /// The delivery notification tag.
        /// </summary>
        public const int DeliveryNotification = -1;

        /// <summary>
        /// The ping tag. Ping DTO's are not buffered on the destination
        /// and used only to test connection.
        /// </summary>
        public const int Ping = -2;

        /// <summary>
        /// Indicates a request for an external endpoint information.
        /// </summary>
        public const int ExternalIPEndPointRequest = -3;

        /// <summary>
        /// Indicates an external endpoint information.
        /// </summary>
        public const int ExternalIPEndPointResponse = -4;
    }
}
