using System;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing.Reduction.ReducibleTypes
{
    public class ReducibleInt64 : Reducible<long>
    {
        public ReducibleInt64(long value) : base(value)
        {
            // Do nothing.
        }

        public override void Add(IReducible other)
        {
            value += ((ReducibleInt64)other).value;
        }

        public override void BitwiseAnd(IReducible other)
        {
            value &= ((ReducibleInt64)other).value;
        }

        public override void BitwiseOr(IReducible other)
        {
            value |= ((ReducibleInt64)other).value;
        }

        public override void BitwiseXor(IReducible other)
        {
            value ^= ((ReducibleInt64)other).value;
        }

        public override void Maximize(IReducible other)
        {
            long otherValue = ((ReducibleInt64)other).value;
            if (otherValue > value)
            {
                value = otherValue;
            }
        }

        public override void Minimize(IReducible other)
        {
            long otherValue = ((ReducibleInt64)other).value;
            if (otherValue < value)
            {
                value = otherValue;
            }
        }

        public override void Multiply(IReducible other)
        {
            value *= ((ReducibleInt64)other).value;
        }
    }
}
