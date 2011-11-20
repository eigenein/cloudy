﻿using System;
using System.Collections.Generic;
using System.Threading;
using Cloudy.Computing.Topologies.Enumerators;
using Cloudy.Computing.Topologies.Enums;
using Cloudy.Computing.Topologies.Structures;

namespace Cloudy.Computing.Topologies
{
    public class StarTopology : Topology
    {
        private int size;

        #region Overrides of Topology

        public override TopologyType TopologyType
        {
            get { return TopologyType.Star; }
        }

        public override void Allocate(int count)
        {
            Interlocked.Add(ref size, count);
        }

        public override IEnumerator<ThreadAddress> GetEnumerator()
        {
            return new StarAddressesEnumerator(size);
        }

        #endregion
    }
}