using System;
using System.Collections.Generic;
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
                || id == StarShortcuts.Peripheral;
        }

        public bool TryAddThread(Guid threadId, ITopologyRepository repository)
        {
            lock (synchronizationRoot)
            {
                repository.AddWellKnownThread(threadId,
                    repository.IsAssigned(threadId, StarShortcuts.Center)
                        ? StarShortcuts.Peripheral
                        : StarShortcuts.Center);
            }
            return true;
        }

        #endregion
    }
}
