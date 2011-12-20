using System;
using Cloudy.Computing.Topologies.Enums;

namespace Cloudy.Computing.Topologies.Interfaces.Master
{
    /// <summary>
    /// Incapsulates master-side topology operations.
    /// </summary>
    public interface ITopology
    {
        TopologyType TopologyType { get; }

        /// <summary>
        /// Adds a thread to the topology.
        /// </summary>
        /// <param name="rank">Returns the rank assigned to the thread.</param>
        /// <returns>
        /// Whether the thread can start, or should be reserved for future.
        /// </returns>
        bool TryAddThread(out byte[] rank);

        /// <summary>
        /// Removes the specified thread from the topology. This may cause 
        /// rank re-assigning.
        /// </summary>
        /// <param name="rank">The thread rank to remove.</param>
        /// <param name="replaceWith">
        /// The thread rank which should replace the removed thread. You should
        /// change the rank of this thread to the rank of the removed thread.
        /// </param>
        /// <returns>Whether the removed rank should be replaced.</returns>
        bool RemoveThread(byte[] rank, out byte[] replaceWith);

        /// <summary>
        /// Updates values in the Remote Access Memory.
        /// </summary>
        void UpdateValues(ITopologyHelper helper);
    }
}
