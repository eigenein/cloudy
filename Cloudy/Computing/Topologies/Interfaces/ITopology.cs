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

        /// <summary>
        /// Gets whether the shortcut is well-known (i.e. a mapped thread ID doesn't depend
        /// on a current thread ID).
        /// </summary>
        bool IsWellKnownShortcut(Guid shortcutId);

        bool TryAddThread(Guid threadId, ITopologyRepository repository);
    }
}
