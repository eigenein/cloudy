using System.Collections.Generic;
using System.Collections.ObjectModel;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Reduction.Delegates;
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
        /// Performs the reduction operation. It combines the values provided 
        /// by each thread, using a specified <paramref name="operation"/>, 
        /// and returns the combined value.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="tag">The user tag.</param>
        /// <param name="value">A value that is provided by the local node.</param>
        /// <param name="operation">The reduce operation.</param>
        /// <param name="targets">Threads to gather values from.</param>
        /// <returns>The combined value.</returns>
        T Reduce<T>(int tag, T value, ReduceOperation operation, IEnumerable<TRank> targets);

        /// <summary>
        /// Performs the reduction operation. It combines the values provided 
        /// by each thread, using a specified <paramref name="reductor"/>, 
        /// and returns the combined value.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="tag">The user tag.</param>
        /// <param name="value">A value that is provided by the local node.</param>
        /// <param name="reductor">The custom reductor.</param>
        /// <param name="targets">Threads to gather values from.</param>
        /// <returns>The combined value.</returns>
        T Reduce<T>(int tag, T value, Reductor reductor, IEnumerable<TRank> targets);

        /// <summary>
        /// Provides a value for a reduction operation.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="tag">The user tag.</param>
        /// <param name="value">The value to combine.</param>
        void Reduce<T>(int tag, T value);

        /// <summary>
        /// Provides a value for a reduction operation.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="tag">The user tag.</param>
        /// <param name="value">The value to combine.</param>
        /// <param name="reductor">The custom reductor.</param>
        void Reduce<T>(int tag, T value, Reductor reductor);

        /// <summary>
        /// Provides a value for a reduction operation.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="tag">The user tag.</param>
        /// <param name="value">The value to combine.</param>
        /// <param name="sender">The rank of a node that should request the reduction operation.</param>
        void Reduce<T>(int tag, T value, TRank sender);

        /// <summary>
        /// Provides a value for a reduction operation.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="tag">The user tag.</param>
        /// <param name="value">The value to combine.</param>
        /// <param name="reductor">The custom reductor.</param>
        /// <param name="sender">The rank of a node that should request the reduction operation.</param>
        void Reduce<T>(int tag, T value, Reductor reductor, TRank sender);

        #endregion

        #region MapReduce

        /// <summary>
        /// Performs the Map-Reduce operation against the nodes.
        /// </summary>
        /// <typeparam name="TValue">The type of source values.</typeparam>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="tag">The user tag.</param>
        /// <param name="value">The source value.</param>
        /// <param name="targets">Target nodes.</param>
        TResult MapReduce<TValue, TResult>(int tag, TValue value, IEnumerable<TRank> targets);

        /// <summary>
        /// Performs a local part of the Map-Reduce operation.
        /// </summary>
        /// <typeparam name="TValue">The type of source values.</typeparam>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="tag">The user tag.</param>
        /// <param name="mapOperation">Map operation.</param>
        /// <param name="reduceOperation">Reduce operation.</param>
        void MapReduce<TValue, TResult>(int tag, MapFunction<TValue, TResult> mapOperation,
            Reductor<TResult> reduceOperation);

        #endregion

        #region Gather

        /// <summary>
        /// Gathers together values from a group of processes.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="senders">Threads to gather value from.</param>
        /// <returns>Combined values from senders.</returns>
        ICollection<T> Gather<T>(IEnumerable<TRank> senders);

        /// <summary>
        /// Gathers together values from a group of processes with current.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="value">Current thread value.</param>
        /// <param name="senders">Threads to gather value from.</param>
        /// <returns>Combined values from senders and current thread.</returns>
        ICollection<T> Gather<T>(T value, IEnumerable<TRank> senders);

        /// <summary>
        /// Send a value for GatherRequest.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="value">Current thread value.</param>
        void Gather<T>(T value);

        #endregion

        #region Scatter

        /// <summary>
        /// Receive value from Scatter operation.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns>Appropriate piece of the data from Scatter operation.</returns>
        T Scatter<T>();

        /// <summary>
        /// Receive value from Scatter operation.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="tag">The user tag.</param>
        /// <returns>Appropriate piece of the data from Scatter operation.</returns>
        T Scatter<T>(int tag);

        /// <summary>
        /// Sends values for the Scatter operation.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="values">Dictionary with ranks and corresponding values for each process in the group.</param>
        /// <param name="recipients">Threads to scatter values to.</param>
        void Scatter<T>(Dictionary<TRank, T> values, IEnumerable<TRank> recipients);

        /// <summary>
        /// Sends values for the Scatter operation.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="tag">The user tag.</param>
        /// <param name="values">Dictionary with ranks and corresponding values for each process in the group.</param>
        /// <param name="recipients">Threads to scatter values to.</param>
        void Scatter<T>(int tag, Dictionary<TRank, T> values, IEnumerable<TRank> recipients);

        #endregion

        #region Utilities

        /// <summary>
        /// Gets the local time of the thread.
        /// </summary>
        /// <returns>Local time of the thread from the start to current time.</returns>
        double Time { get; }

        #endregion
    }
}
