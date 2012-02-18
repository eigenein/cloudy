using System;
using System.Text;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing.Reduction.ReducibleTypes
{
    public class StringReducible : Reducible
    {
        private string value;

        public StringReducible(string value)
        {
            this.value = value;
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

        #region Implementation of IReducible<string>

        public string Value
        {
            get { return value; }
        }

        #endregion
    }
}
