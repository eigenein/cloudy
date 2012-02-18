using System;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing.Reduction.ReducibleTypes
{
    public class ReducibleDecimal : Reducible<decimal>
    {
        public ReducibleDecimal(decimal value) : base(value)
        {
            // Do nothing.
        }

        public override void Add(IReducible other)
        {
            value += ((ReducibleDecimal)other).value;
        }

        public override void Maximize(IReducible other)
        {
            decimal otherValue = ((ReducibleDecimal)other).value;
            if (otherValue > value)
            {
                value = otherValue;
            }
        }

        public override void Minimize(IReducible other)
        {
            decimal otherValue = ((ReducibleDecimal)other).value;
            if (otherValue < value)
            {
                value = otherValue;
            }
        }

        public override void Multiply(IReducible other)
        {
            value *= ((ReducibleDecimal)other).value;
        }
    }
}
