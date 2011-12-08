using System;

namespace Cloudy.Computing.Enums
{
    public enum EnvironmentOperationType
    {
        /// <summary>
        /// Fake operation type for errors diagnostics. It should be never used
        /// in real operations.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Peer-To-Peer single message.
        /// </summary>
        PeerToPeer = 1
    }
}