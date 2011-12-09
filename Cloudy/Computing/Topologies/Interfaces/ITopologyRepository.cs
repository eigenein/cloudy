using System;
using System.Collections.Generic;

namespace Cloudy.Computing.Topologies.Interfaces
{
    /// <summary>
    /// Describes a repository for storing a topology structure.
    /// </summary>
    public interface ITopologyRepository
    {
        bool TryGetThreadsByShortcut(Guid currentThreadId, Guid shortcutId, 
            out ICollection<Guid> targetThreadsIds);

        void AddThread(Guid currentThreadId, Guid shortcutId, Guid targetThreadId);

        void AddWellKnownThread(Guid threadId, Guid wellKnownShortcutId);

        bool IsDefined(Guid wellKnownShortcutId);
    }
}
