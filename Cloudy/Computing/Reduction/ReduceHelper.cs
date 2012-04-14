using System;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Reduction.Delegates;

namespace Cloudy.Computing.Reduction
{
    public static class ReduceHelper<TValue>
    {
        public static TValue Reduce(TValue value1, TValue value2, ReduceOperation operation)
        {
            return Reduce(value1, value2, ReductorsCache.Get(operation));
        }

        public static TValue Reduce(TValue value1, TValue value2, Reductor reductor)
        {
            if (reductor == null)
            {
                throw new ArgumentNullException("reductor", "Specify the reductor.");
            }
            IReducible reducible1 = Reducible.Create(value1);
            IReducible reducible2 = Reducible.Create(value2);
            reductor(reducible1, reducible2);
            return ((Reducible<TValue>)reducible1).Value;
        }
    }
}
