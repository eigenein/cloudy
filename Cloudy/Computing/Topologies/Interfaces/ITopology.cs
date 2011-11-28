using System;

namespace Cloudy.Computing.Topologies.Interfaces
{
    public interface ITopology
    {
        bool IsWellKnownShortcut(Guid shortcutId);

        bool TryAddThread(Guid threadId, ITopologyRepository repository);
    }
}
