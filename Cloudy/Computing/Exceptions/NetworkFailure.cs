using System;

namespace Cloudy.Computing.Exceptions
{
    /// <summary>
    /// Means that the entire computing process is stopped and the network is
    /// dropped.
    /// </summary>
    [Serializable]
    public class NetworkFailure : Exception
    {
        public NetworkFailure(string message)
            : base(message)
        {
            // Do nothing.
        }
    }
}
