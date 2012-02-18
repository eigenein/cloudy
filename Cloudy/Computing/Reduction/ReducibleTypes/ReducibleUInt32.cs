using System;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing.Reduction.ReducibleTypes
{
    public class ReducibleUInt32 : Reducible<uint>
    {
        public ReducibleUInt32(uint value) 
            : base(value)
        {
            // Do nothing.
        }

        public override void Add(IReducible other)
        {
            value += ((ReducibleUInt32)other).value;
        }

        public override void BitwiseAnd(IReducible other)
        {
            value &= ((ReducibleUInt32)other).value;
        }

        public override void BitwiseOr(IReducible other)
        {
            value |= ((ReducibleUInt32)other).value;
        }

        public override void BitwiseXor(IReducible other)
        {
            value ^= ((ReducibleUInt32)other).value;
        }

        public override void Maximize(IReducible other)
        {
            uint otherValue = ((ReducibleUInt32)other).value;
            if (otherValue > value)
            {
                value = otherValue;
            }
        }

        public override void Minimize(IReducible other)
        {
            uint otherValue = ((ReducibleUInt32)other).value;
            if (otherValue < value)
            {
                value = otherValue;
            }
        }

        public override void Multiply(IReducible other)
        {
            value *= ((ReducibleUInt32)other).value;
        }
    }
}
