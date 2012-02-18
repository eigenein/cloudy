using System;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing.Reduction.ReducibleTypes
{
    public class ReducibleByte : Reducible<byte>
    {
        public ReducibleByte(byte value) 
            : base(value)
        {
            // Do nothing.
        }

        #region Implementation of IReducible

        public override void Add(IReducible other)
        {
            value += ((ReducibleByte)other).value;
        }

        public override void Multiply(IReducible other)
        {
            value *= ((ReducibleByte)other).value;
        }

        public override void BitwiseAnd(IReducible other)
        {
            value &= ((ReducibleByte)other).value;
        }

        public override void BitwiseOr(IReducible other)
        {
            value |= ((ReducibleByte)other).value;
        }

        public override void BitwiseXor(IReducible other)
        {
            value ^= ((ReducibleByte)other).value;
        }

        public override void Maximize(IReducible other)
        {
            byte otherValue = ((ReducibleByte)other).value;
            if (otherValue > value)
            {
                value = otherValue;
            }
        }

        public override void Minimize(IReducible other)
        {
            byte otherValue = ((ReducibleByte)other).value;
            if (otherValue < value)
            {
                value = otherValue;
            }
        }

        #endregion
    }
}
