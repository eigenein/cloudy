using System;
using System.Collections.Generic;
using System.Linq;
using Cloudy.Computing.Topologies.Interfaces;
using Cloudy.Computing.Topologies.Shortcuts;

namespace Cloudy.Computing.Topologies
{
    /// <summary>
    /// Implements the Star topology.
    /// </summary>
    public class StarTopology : ITopology
    {
        private readonly object synchronizationRoot = new object();

        #region Implementation of ITopology

        public bool IsShortcut(Guid id)
        {
            return id == StarShortcuts.Center 
                || id == StarShortcuts.Peripherals;
        }

        public bool TryAddThread(Guid threadId, ITopologyRepository repository)
        {
            lock (synchronizationRoot)
            {
                repository.AddWellKnownThread(threadId,
                    repository.IsDefined(StarShortcuts.Center)
                        ? StarShortcuts.Peripherals
                        : StarShortcuts.Center);
            }
            return true;
        }

        public bool TryGetRoute(Guid currentThreadId, Guid destinationThreadId,
            ITopologyRepository repository, out Guid nextThreadId)
        {
            ICollection<Guid> targetThreadsIds;
            if (repository.TryGetThreadsByShortcut(currentThreadId, StarShortcuts.Center,
                out targetThreadsIds))
            {
                Guid centerThreadId = targetThreadsIds.First();
                nextThreadId = centerThreadId == currentThreadId ?
                    destinationThreadId : centerThreadId;
                return true;
            }
            nextThreadId = Guid.Empty;
            return false;
        }

        #endregion
    }
}
