using System;

namespace Cloudy.Computing.Interfaces
{
    /// <summary>
    /// Describes a computing thread on a slave side.
    /// </summary>
    public interface IComputingThread
    {
        /// <summary>
        /// Executes a code of the thread.
        /// </summary>
        void Run(Guid threadId, IEnvironment environment);
    }
}
