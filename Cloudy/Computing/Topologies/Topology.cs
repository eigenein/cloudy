using System;
using System.Collections;
using System.Collections.Generic;
using Cloudy.Computing.Topologies.Enums;
using Cloudy.Computing.Topologies.Structures;

namespace Cloudy.Computing.Topologies
{
    public abstract class Topology : IEnumerable<ThreadAddress>
    {
        public abstract TopologyType TopologyType { get; }

        public abstract void Allocate(int count);

        #region Implementation of IEnumerable

        /// <summary>
        /// Gets the "active" addresses enumerator. This means that not all the
        /// allocated threads can take a part in the computation, because their
        /// presence can violate a topology structure.
        /// </summary>
        public abstract IEnumerator<ThreadAddress> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
