using System;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing.Reduction.ReducibleTypes
{
    public class ReducibleUInt16 : Reducible<ushort>
    {
        public ReducibleUInt16(ushort value)
            : base(value)
        {
            // Do nothing.
        }

        public override void Add(IReducible other)
        {
            value += ((ReducibleUInt16)other).value;
        }

        public override void BitwiseAnd(IReducible other)
        {
            value &= ((ReducibleUInt16)other).value;
        }

        public override void BitwiseOr(IReducible other)
        {
            value |= ((ReducibleUInt16)other).value;
        }

        public override void BitwiseXor(IReducible other)
        {
            value ^= ((ReducibleUInt16)other).value;
        }

        public override void Maximize(IReducible other)
        {
            ushort otherValue = ((ReducibleUInt16)other).value;
            if (otherValue > value)
            {
                value = otherValue;
            }
        }

        public override void Minimize(IReducible other)
        {
            ushort otherValue = ((ReducibleUInt16)other).value;
            if (otherValue < value)
            {
                value = otherValue;
            }
        }

        public override void Multiply(IReducible other)
        {
            value *= ((ReducibleUInt16)other).value;
        }
    }
}
