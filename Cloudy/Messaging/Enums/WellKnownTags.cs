using System;

namespace Cloudy.Messaging.Enums
{
    /// <summary>
    /// Defines the well-known tags.
    /// </summary>
    public static class WellKnownTags
    {
        /// <summary>
        /// The delivery notification tag.
        /// </summary>
        public const int DeliveryNotification = 0;

        /// <summary>
        /// The ping tag. Ping DTO's are not buffered on the destination
        /// and used only to test connection.
        /// </summary>
        public const int Ping = 1;
    }
}
