using System;

namespace Cloudy.Computing.Enums
{
    public enum SlaveState
    {
        /// <summary>
        /// The initial state.
        /// </summary>
        NotJoined,

        /// <summary>
        /// A slave is joined and processing incoming messages.
        /// </summary>
        Joined,

        /// <summary>
        /// A slave has left the network.
        /// </summary>
        Left
    }
}
