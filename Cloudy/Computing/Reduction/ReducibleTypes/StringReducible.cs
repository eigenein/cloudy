using System;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing.Reduction.ReducibleTypes
{
    public class StringReducible : Reducible<string>
    {
        public StringReducible(string value) : base(value)
        {
            // Do nothing.
        }

        #region Implementation of IReducible

        public override void Add(IReducible other)
        {
            value += ((StringReducible)other).value;
        }

        public override void Maximize(IReducible other)
        {
            string otherString = ((StringReducible)other).Value;
            if (value.CompareTo(otherString) > 0)
            {
                value = otherString;
            }
        }

        public override void Minimize(IReducible other)
        {
            string otherString = ((StringReducible)other).Value;
            if (value.CompareTo(otherString) < 0)
            {
                value = otherString;
            }
        }

        #endregion
    }
}
