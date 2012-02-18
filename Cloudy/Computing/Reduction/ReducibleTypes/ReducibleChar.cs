using System;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing.Reduction.ReducibleTypes
{
    public class ReducibleChar : Reducible<Char>
    {
        public ReducibleChar(char value) : base(value)
        {
            // Do nothing.
        }

        public override void Maximize(IReducible other)
        {
            char otherValue = ((ReducibleChar)other).value;
            if (value.CompareTo(otherValue) < 0)
            {
                value = otherValue;
            }
        }

        public override void Minimize(IReducible other)
        {
            char otherValue = ((ReducibleChar)other).value;
            if (value.CompareTo(otherValue) > 0)
            {
                value = otherValue;
            }
        }
    }
}
