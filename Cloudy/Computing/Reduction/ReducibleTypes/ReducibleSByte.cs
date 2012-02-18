using System;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing.Reduction.ReducibleTypes
{
    public class ReducibleSByte : Reducible<sbyte>
    {
        public ReducibleSByte(sbyte value) 
            : base(value)
        {
            // Do nothing.
        }

        #region Implementation of IReducible

        public override void Add(IReducible other)
        {
            value += ((ReducibleSByte)other).value;
        }

        public override void Multiply(IReducible other)
        {
            value *= ((ReducibleSByte)other).value;
        }

        public override void BitwiseAnd(IReducible other)
        {
            value &= ((ReducibleSByte)other).value;
        }

        public override void BitwiseOr(IReducible other)
        {
            value |= ((ReducibleSByte)other).value;
        }

        public override void BitwiseXor(IReducible other)
        {
            value ^= ((ReducibleSByte)other).value;
        }

        public override void Maximize(IReducible other)
        {
            sbyte otherValue = ((ReducibleSByte)other).value;
            if (otherValue > value)
            {
                value = otherValue;
            }
        }

        public override void Minimize(IReducible other)
        {
            sbyte otherValue = ((ReducibleSByte)other).value;
            if (otherValue < value)
            {
                value = otherValue;
            }
        }

        #endregion
    }
}
