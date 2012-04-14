using System;
using System.Collections.Generic;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Reduction.Delegates;

namespace Cloudy.Computing.Reduction
{
    /// <summary>
    /// Contains the common reductors.
    /// </summary>
    public static class ReductorsCache
    {
        private static readonly IDictionary<ReduceOperation, Reductor> Cache =
            new Dictionary<ReduceOperation, Reductor>();

        static ReductorsCache()
        {
            Cache.Add(ReduceOperation.BitwiseAnd, 
                (accumulatedValue, nextValue) => accumulatedValue.BitwiseAnd(nextValue));
            Cache.Add(ReduceOperation.BitwiseOr,
                (accumulatedValue, nextValue) => accumulatedValue.BitwiseOr(nextValue));
            Cache.Add(ReduceOperation.BitwiseXor,
                (accumulatedValue, nextValue) => accumulatedValue.BitwiseXor(nextValue));
            Cache.Add(ReduceOperation.LogicalAnd,
                (accumulatedValue, nextValue) => accumulatedValue.LogicalAnd(nextValue));
            Cache.Add(ReduceOperation.LogicalOr,
                (accumulatedValue, nextValue) => accumulatedValue.LogicalOr(nextValue));
            Cache.Add(ReduceOperation.LogicalXor,
                (accumulatedValue, nextValue) => accumulatedValue.LogicalXor(nextValue));
            Cache.Add(ReduceOperation.Maximum,
                (accumulatedValue, nextValue) => accumulatedValue.Maximize(nextValue));
            Cache.Add(ReduceOperation.Minimum,
                (accumulatedValue, nextValue) => accumulatedValue.Minimize(nextValue));
            Cache.Add(ReduceOperation.Product,
                (accumulatedValue, nextValue) => accumulatedValue.Multiply(nextValue));
            Cache.Add(ReduceOperation.Sum,
                (accumulatedValue, nextValue) => accumulatedValue.Add(nextValue));
        }

        public static Reductor Get(ReduceOperation operation)
        {
            if (operation == ReduceOperation.Custom)
            {
                throw new NotSupportedException(
                    "Custom reduce operation must be specified explicitly.");
            }
            return Cache[operation];
        }
    }
}
