﻿using System;

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
        public const int DeliveryNotification = -1;

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
    }
}
