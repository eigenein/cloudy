using System;

namespace Cloudy.Computing.Enums
{
    /// <summary>
    /// Represents a slave thread state.
    /// </summary>
    public enum ThreadState
    {
        /// <summary>
        /// The thread is created on a slave side, added to the current topology,
        /// but currently not running.
        /// </summary>
        NotRunning,

        /// <summary>
        /// The thread is created on a slave side, but cannot be added to the
        /// current topology and thus reserved for future use.
        /// </summary>
        Reserved,

        /// <summary>
        /// Running a computing code.
        /// </summary>
        Running
    }
}