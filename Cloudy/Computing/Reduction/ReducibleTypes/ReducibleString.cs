using System;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing.Reduction.ReducibleTypes
{
    public class ReducibleString : Reducible<string>
    {
        public ReducibleString(string value) : base(value)
        {
            // Do nothing.
        }

        #region Implementation of IReducible

        public override void Add(IReducible other)
        {
            value += ((ReducibleString)other).value;
        }

        public override void Maximize(IReducible other)
        {
            string otherString = ((ReducibleString)other).Value;
            if (value.CompareTo(otherString) < 0)
            {
                value = otherString;
            }
        }

        public override void Minimize(IReducible other)
        {
            string otherString = ((ReducibleString)other).Value;
            if (value.CompareTo(otherString) > 0)
            {
                value = otherString;
            }
        }

        #endregion
    }
}
