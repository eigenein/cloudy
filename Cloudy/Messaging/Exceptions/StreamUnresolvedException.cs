using System;

namespace Cloudy.Messaging.Exceptions
{
    /// <summary>
    /// Occurs that the sender ID or recipient ID was unresolved to a stream.
    /// </summary>
    [Serializable]
    public class StreamUnresolvedException : Exception
    {
        public StreamUnresolvedException(Guid unresolvedId)
            : base(String.Format("The following ID was unresolved to a stream: {0}", unresolvedId))
        {
            // Do nothing.
        }
    }
}
