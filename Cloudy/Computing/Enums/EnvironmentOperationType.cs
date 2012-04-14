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
        PeerToPeer = 1,

        /// <summary>
        /// A request for a reduction operation.
        /// </summary>
        ReduceRequest = 2,

        /// <summary>
        /// A response that handles the current value of the reduction operation.
        /// </summary>
        ReduceResponse = 3,

        /// <summary>
        /// A request for a MapReduce operation.
        /// </summary>
        MapReduceRequest = 4,

        /// <summary>
        /// A response that handles the current value of the MapReduce operation.
        /// </summary>
        MapReduceResponse = 5
    }
}