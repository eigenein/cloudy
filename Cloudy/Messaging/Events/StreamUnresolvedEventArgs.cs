using System;

namespace Cloudy.Messaging.Events
{
    /// <summary>
    /// Notifies that the sender ID or recipient ID was unresolved to a stream.
    /// </summary>
    public class StreamUnresolvedEventArgs : EventArgs
    {
        public StreamUnresolvedEventArgs(Guid unresolvedId)
        {
            this.UnresolvedId = unresolvedId;
        }

        /// <summary>
        /// The source or the recipient ID.
        /// </summary>
        public Guid UnresolvedId { get; private set; }
    }
}
