using System;
using Cloudy.Computing.Topologies.Interfaces;

namespace Cloudy.Computing.Topologies
{
    public abstract class Topology
    {
        protected Topology UnderlyingTopology;

        protected ITopologyRepository Repository;

        public Topology(ITopologyRepository repository)
        {
            this.Repository = repository;
        }

        public Topology(ITopologyRepository repository,
            Topology underlyingTopology) : this(repository)
        {
            this.UnderlyingTopology = underlyingTopology;
        }
    }
}
