using System;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing.Reduction.ReducibleTypes
{
    public class ReducibleInt32 : Reducible<int>
    {
        public ReducibleInt32(int value) 
            : base(value)
        {
            // Do nothing.
        }

        public override void Add(IReducible other)
        {
            value += ((ReducibleInt32)other).value;
        }

        public override void BitwiseAnd(IReducible other)
        {
            value &= ((ReducibleInt32)other).value;
        }

        public override void BitwiseOr(IReducible other)
        {
            value |= ((ReducibleInt32)other).value;
        }

        public override void BitwiseXor(IReducible other)
        {
            value ^= ((ReducibleInt32)other).value;
        }

        public override void Maximize(IReducible other)
        {
            int otherValue = ((ReducibleInt32)other).value;
            if (otherValue > value)
            {
                value = otherValue;
            }
        }

        public override void Minimize(IReducible other)
        {
            int otherValue = ((ReducibleInt32)other).value;
            if (otherValue < value)
            {
                value = otherValue;
            }
        }

        public override void Multiply(IReducible other)
        {
            value *= ((ReducibleInt32)other).value;
        }
    }
}
