using System;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing.Reduction.ReducibleTypes
{
    public class ByteReducible : Reducible<byte>
    {
        public ByteReducible(byte value) 
            : base(value)
        {
            // Do nothing.
        }

        #region Implementation of IReducible

        public override void Add(IReducible other)
        {
            value += ((ByteReducible)other).value;
        }

        public override void Multiply(IReducible other)
        {
            value *= ((ByteReducible)other).value;
        }

        public override void BitwiseAnd(IReducible other)
        {
            value &= ((ByteReducible)other).value;
        }

        public override void BitwiseOr(IReducible other)
        {
            value |= ((ByteReducible)other).value;
        }

        public override void BitwiseXor(IReducible other)
        {
            value ^= ((ByteReducible)other).value;
        }

        public override void Maximize(IReducible other)
        {
            byte otherValue = ((ByteReducible)other).value;
            if (otherValue > value)
            {
                value = otherValue;
            }
        }

        public override void Minimize(IReducible other)
        {
            byte otherValue = ((ByteReducible)other).value;
            if (otherValue < value)
            {
                value = otherValue;
            }
        }

        #endregion
    }
}
