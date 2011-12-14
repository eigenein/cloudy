using System;
using Cloudy.Computing.Topologies.Interfaces;

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
        void Run(IEnvironment environment);
    }

    /// <summary>
    /// Describes a computing thread on a slave side.
    /// </summary>
    public interface IComputingThread<TRank>
        where TRank : IRank
    {
        /// <summary>
        /// Executes a code of the thread.
        /// </summary>
        void Run(IEnvironment<TRank> environment);
    }
}
