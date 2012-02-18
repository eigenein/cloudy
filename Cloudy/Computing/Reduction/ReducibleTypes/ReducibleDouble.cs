using System;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing.Reduction.ReducibleTypes
{
    public class ReducibleDouble : Reducible<double>
    {
        public ReducibleDouble(double value) : base(value)
        {
            // Do nothing.
        }

        public override void Add(IReducible other)
        {
            value += ((ReducibleDouble)other).value;
        }

        public override void Maximize(IReducible other)
        {
            double otherValue = ((ReducibleDouble)other).value;
            if (otherValue > value)
            {
                value = otherValue;
            }
        }

        public override void Minimize(IReducible other)
        {
            double otherValue = ((ReducibleDouble)other).value;
            if (otherValue < value)
            {
                value = otherValue;
            }
        }

        public override void Multiply(IReducible other)
        {
            value *= ((ReducibleDouble)other).value;
        }
    }
}
