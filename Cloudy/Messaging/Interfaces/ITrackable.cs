using System;

namespace Cloudy.Messaging.Interfaces
{
    /// <summary>
    /// Represents an entity that can be tracked within a session.
    /// </summary>
    public interface ITrackable
    {
        /// <summary>
        /// Gets or sets the source of the entity.
        /// </summary>
        Guid FromId { get; set; }

        /// <summary>
        /// Gets or sets the tracking ID of the message.
        /// </summary>
        long TrackingId { get; set; }
    }
}
