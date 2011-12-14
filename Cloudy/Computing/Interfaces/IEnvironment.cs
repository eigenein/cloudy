using System;
using System.Collections.Generic;
using Cloudy.Computing.Topologies.Interfaces;

namespace Cloudy.Computing.Interfaces
{
    /// <summary>
    /// Represents an environment of a computing thread.
    /// </summary>
    public interface IEnvironment
    {
        // Nothing.
    }

    /// <summary>
    /// Represents an environment of a computing thread.
    /// </summary>
    public interface IEnvironment<TRank> : IEnvironment
        where TRank : IRank
    {
        TRank Rank { get; }

        void Send<T>(int tag, T value, TRank recipient);

        void Send<T>(int tag, T value, ICollection<TRank> recipients);

        void Receive<T>(int tag, out T value, out TRank sender);

        void Receive<T>(out int tag, out T value, out TRank sender);

        void Receive<T>(int tag, out T value, TRank sender);

        void Receive<T>(out int tag, out T value, TRank sender);
    }
}
