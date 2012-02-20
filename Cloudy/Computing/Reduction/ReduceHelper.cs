using System;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing.Reduction
{
    public static class ReduceHelper<TValue>
    {
        public static TValue Reduce(TValue value1, TValue value2, ReduceOperation operation)
        {
            IReducible reducible1 = Reducible.Create(value1);
            IReducible reducible2 = Reducible.Create(value2);
            Action<IReducible, IReducible> reductor = ReductorsCache.Get(operation);
            reductor(reducible1, reducible2);
            return ((Reducible<TValue>)reducible1).Value;
        }
    }
}
