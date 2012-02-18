using System;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing.Reduction.ReducibleTypes
{
    public class ReducibleSingle : Reducible<float>
    {
        public ReducibleSingle(float value) : base(value)
        {
            // Do nothing.
        }

        public override void Add(IReducible other)
        {
            value += ((ReducibleSingle)other).value;
        }

        public override void Maximize(IReducible other)
        {
            float otherValue = ((ReducibleSingle)other).value;
            if (otherValue > value)
            {
                value = otherValue;
            }
        }

        public override void Minimize(IReducible other)
        {
            float otherValue = ((ReducibleSingle)other).value;
            if (otherValue < value)
            {
                value = otherValue;
            }
        }

        public override void Multiply(IReducible other)
        {
            value *= ((ReducibleSingle)other).value;
        }
    }
}
