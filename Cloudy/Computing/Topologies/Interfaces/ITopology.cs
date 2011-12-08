using System;

namespace Cloudy.Computing.Topologies.Interfaces
{
    /// <summary>
    /// Incapsulates a server-side topology operations.
    /// </summary>
    public interface ITopology
    {
        /// <summary>
        /// Gets whether the specified ID is a shortcut in this topology.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool IsShortcut(Guid id);

        bool TryAddThread(Guid threadId, ITopologyRepository repository);
    }
}
