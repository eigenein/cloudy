using System;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing.Reduction.ReducibleTypes
{
    public class ReducibleUInt64 : Reducible<ulong>
    {
        public ReducibleUInt64(ulong value) : base(value)
        {
            // Do nothing.
        }

        public override void Add(IReducible other)
        {
            value += ((ReducibleUInt64)other).value;
        }

        public override void BitwiseAnd(IReducible other)
        {
            value &= ((ReducibleUInt64)other).value;
        }

        public override void BitwiseOr(IReducible other)
        {
            value |= ((ReducibleUInt64)other).value;
        }

        public override void BitwiseXor(IReducible other)
        {
            value ^= ((ReducibleUInt64)other).value;
        }

        public override void Maximize(IReducible other)
        {
            ulong otherValue = ((ReducibleUInt64)other).value;
            if (otherValue > value)
            {
                value = otherValue;
            }
        }

        public override void Minimize(IReducible other)
        {
            ulong otherValue = ((ReducibleUInt64)other).value;
            if (otherValue < value)
            {
                value = otherValue;
            }
        }

        public override void Multiply(IReducible other)
        {
            value *= ((ReducibleUInt64)other).value;
        }
    }
}
