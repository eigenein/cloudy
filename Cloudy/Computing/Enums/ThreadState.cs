using System;

namespace Cloudy.Computing.Enums
{
    public enum ThreadState
    {
        /// <summary>
        /// The thread is about to be allocated.
        /// </summary>
        Initial,

        /// <summary>
        /// The addresses are assigned.
        /// </summary>
        Allocated,
        
        /// <summary>
        /// The addresses are assigned and interconnections are established.
        /// </summary>
        Ready,

        /// <summary>
        /// The thread is running. :)
        /// </summary>
        Running
    }
}
