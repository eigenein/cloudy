using System;

namespace Cloudy.Computing.Interfaces
{
    public interface IReducible
    {
        void Add(IReducible other);

        void Multiply(IReducible other);

        void LogicalAnd(IReducible other);

        void BitwiseAnd(IReducible other);

        void LogicalOr(IReducible other);

        void BitwiseOr(IReducible other);

        void LogicalXor(IReducible other);

        void BitwiseXor(IReducible other);

        void Maximize(IReducible other);

        void Minimize(IReducible other);
    }

    public interface IReducible<TValue> : IReducible
    {
        TValue Value { get; }
    }
}
