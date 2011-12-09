using System;
using System.Collections.Generic;
using Cloudy.Computing.Topologies.Interfaces;

namespace Cloudy.Computing.Topologies
{
    public class TopologyRepository : ITopologyRepository
    {
        private readonly object wellKnownLocker = new object();

        private readonly Dictionary<Guid, HashSet<Guid>> wellKnownThreads =
            new Dictionary<Guid, HashSet<Guid>>();

        private readonly object locker = new object();

        private readonly Dictionary<Guid, Dictionary<Guid, HashSet<Guid>>> threads =
            new Dictionary<Guid, Dictionary<Guid, HashSet<Guid>>>();

        #region Implementation of ITopologyRepository

        public bool TryGetThreadsByShortcut(Guid currentThreadId, Guid shortcutId, 
            out ICollection<Guid> targetThreadsIds)
        {
            HashSet<Guid> threadsIds;
            lock (wellKnownLocker)
            {
                if (wellKnownThreads.TryGetValue(shortcutId, out threadsIds))
                {
                    targetThreadsIds = threadsIds;
                    return true;
                }
            }
            lock (locker)
            {
                targetThreadsIds = null;
                Dictionary<Guid, HashSet<Guid>> shortcuts;
                if (!threads.TryGetValue(currentThreadId, out shortcuts))
                {
                    return false;
                }
                if (!shortcuts.TryGetValue(shortcutId, out threadsIds))
                {
                    return false;
                }
                targetThreadsIds = threadsIds;
                return true;
            }
        }

        public void AddThread(Guid currentThreadId, Guid shortcutId, Guid targetThreadId)
        {
            lock (locker)
            {
                Dictionary<Guid, HashSet<Guid>> shortcuts;
                if (!threads.TryGetValue(currentThreadId, out shortcuts))
                {
                    shortcuts = threads[currentThreadId] = new Dictionary<Guid, HashSet<Guid>>();
                }
                HashSet<Guid> targetThreadsIds;
                if (!shortcuts.TryGetValue(shortcutId, out targetThreadsIds))
                {
                    targetThreadsIds = shortcuts[shortcutId] = new HashSet<Guid>();
                }
                targetThreadsIds.Add(targetThreadId);
            }
        }

        public void AddWellKnownThread(Guid threadId, Guid wellKnownShortcutId)
        {
            lock (wellKnownLocker)
            {
                HashSet<Guid> threadsIds;
                if (!wellKnownThreads.TryGetValue(wellKnownShortcutId, out threadsIds))
                {
                    threadsIds = wellKnownThreads[wellKnownShortcutId] = 
                        new HashSet<Guid>();
                }
                threadsIds.Add(threadId);
            }
        }

        public bool IsDefined(Guid wellKnownShortcutId)
        {
            return wellKnownThreads.ContainsKey(wellKnownShortcutId);
        }

        #endregion
    }
}
