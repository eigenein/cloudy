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

        #region Reduction

        /// <summary>
        /// Performs the reduction operation. It combines the values provided 
        /// by each thread, using a specified <paramref name="operation"/>, 
        /// and returns the combined value.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="tag">The user tag.</param>
        /// <param name="operation">The reduce operation.</param>
        /// <param name="targets">Threads to gather values from.</param>
        /// <returns>The combined value.</returns>
        T Reduce<T>(int tag, ReduceOperation operation, IEnumerable<TRank> targets);

        /// <summary>
        /// Provides a value for a reduction operation.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="tag">The user tag.</param>
        /// <param name="value">The value to combine.</param>
        void Reduce<T>(int tag, T value);

        #endregion
    }
}
