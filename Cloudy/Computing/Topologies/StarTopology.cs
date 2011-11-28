using System;
using System.Collections.Generic;
using Cloudy.Computing.Topologies.Interfaces;
using Cloudy.Computing.Topologies.Shortcuts;

namespace Cloudy.Computing.Topologies
{
    public class StarTopology : ITopology
    {
        private readonly object locker = new object();

        #region Implementation of ITopology

        public bool IsWellKnownShortcut(Guid shortcutId)
        {
            return shortcutId == StarShortcuts.Center;
        }

        public bool TryAddThread(Guid threadId, ITopologyRepository repository)
        {
            lock (locker)
            {
                ICollection<Guid> centersIds;
                if (repository.TryGetThreadsByShortcut(StarShortcuts.Center, out centersIds))
                {
                    foreach (Guid centerId in centersIds)
                    {
                        repository.AddThread(centerId, StarShortcuts.Peripheral, threadId);
                    }
                }
                else
                {
                    repository.AddWellKnownThread(threadId, StarShortcuts.Center);
                }
            }
            return true;
        }

        #endregion
    }
}
