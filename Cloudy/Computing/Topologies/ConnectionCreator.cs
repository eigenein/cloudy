using System;
using Cloudy.Computing.Topologies.Interfaces;

namespace Cloudy.Computing.Topologies
{
    public abstract class ConnectionCreator
    {
        protected ConnectionCreator UnderlyingCreator;

        protected IConnectionsRepository Repository;

        public ConnectionCreator(IConnectionsRepository repository)
        {
            this.Repository = repository;
        }

        public ConnectionCreator(IConnectionsRepository repository,
            ConnectionCreator underlyingCreator) : this(repository)
        {
            this.UnderlyingCreator = underlyingCreator;
        }
    }
}
