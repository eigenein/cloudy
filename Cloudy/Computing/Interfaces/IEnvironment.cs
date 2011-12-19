using System;
using System.Collections.Generic;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Topologies.Interfaces;

namespace Cloudy.Computing.Interfaces
{
    /// <summary>
    /// Represents an environment of a computing thread.
    /// </summary>
    public interface IEnvironment
    {
        #region Remote Memory Access

        void SetRemoteValue<TValue>(byte[] @namespace, string key, TValue value, 
            TimeToLive timeToLive);

        bool TryGetRemoteValue<TValue>(byte[] @namespace, string key, out TValue value);

        #endregion
    }

    /// <summary>
    /// Represents an environment of a computing thread.
    /// </summary>
    public interface IEnvironment<TRank> : IEnvironment
        where TRank : IRank
    {
        TRank Rank { get; }

        #region Send

        void Send<T>(int tag, T value, TRank recipient);

        void Send<T>(int tag, T value, IEnumerable<TRank> recipients);

        #endregion

        #region Receive

        void Receive<T>(int tag, out T value, out TRank sender);

        void Receive<T>(out int tag, out T value, out TRank sender);

        void Receive<T>(int tag, out T value, TRank sender);

        void Receive<T>(out int tag, out T value, TRank sender);

        #endregion
    }
}
