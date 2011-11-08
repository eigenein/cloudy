using System;

namespace Cloudy.Messaging.Interfaces
{
    public interface ITrackable
    {
        Guid FromId { get; set; }

        long TrackingId { get; set; }
    }
}
