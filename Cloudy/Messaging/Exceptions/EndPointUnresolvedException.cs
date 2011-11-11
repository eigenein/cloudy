using System;

namespace Cloudy.Messaging.Exceptions
{
    /// <summary>
    /// Occurs that the sender ID or recipient ID was unresolved to an endpoint.
    /// </summary>
    [Serializable]
    public class EndPointUnresolvedException : Exception
    {
        public EndPointUnresolvedException(Guid unresolvedId)
            : base(String.Format("The following ID was unresolved to an endpoint: {0}", unresolvedId))
        {
            // Do nothing.
        }
    }
}
