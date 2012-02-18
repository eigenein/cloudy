using System;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing.Reduction.ReducibleTypes
{
    public class ReducibleInt16 : Reducible<short>
    {
        public ReducibleInt16(short value)
            : base(value)
        {
            // Do nothing.
        }

        public override void Add(IReducible other)
        {
            value += ((ReducibleInt16)other).value;
        }

        public override void BitwiseAnd(IReducible other)
        {
            value &= ((ReducibleInt16)other).value;
        }

        public override void BitwiseOr(IReducible other)
        {
            value |= ((ReducibleInt16)other).value;
        }

        public override void BitwiseXor(IReducible other)
        {
            value ^= ((ReducibleInt16)other).value;
        }

        public override void Maximize(IReducible other)
        {
            short otherValue = ((ReducibleInt16)other).value;
            if (otherValue > value)
            {
                value = otherValue;
            }
        }

        public override void Minimize(IReducible other)
        {
            short otherValue = ((ReducibleInt16)other).value;
            if (otherValue < value)
            {
                value = otherValue;
            }
        }

        public override void Multiply(IReducible other)
        {
            value *= ((ReducibleInt16)other).value;
        }
    }
}
