using System;
using System.Collections.Generic;

namespace Cloudy.Computing.Topologies.Interfaces
{
    public interface ITopologyRepository
    {
        bool TryGetThreadsByShortcut(Guid wellKnownShortcutId, 
            out ICollection<Guid> targetThreadsIds);

        void AddThread(Guid currentThreadId, Guid shortcutId, Guid targetThreadId);

        void AddWellKnownThread(Guid threadId, Guid wellKnownShortcutId);
    }
}
