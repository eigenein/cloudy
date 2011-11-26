using System;

namespace Cloudy.Helpers
{
    public class EventArgs<T> : EventArgs
    {
        private readonly T value;

        public EventArgs(T value)
        {
            this.value = value;
        }

        public T Value
        {
            get { return value; }
        }
    }

    public class EventArgs<T1, T2> : EventArgs
    {
        private readonly T1 value1;

        private readonly T2 value2;

        public EventArgs(T1 value1, T2 value2)
        {
            this.value1 = value1;
            this.value2 = value2;
        }

        public T1 Value1
        {
            get { return value1; }
        }

        public T2 Value2
        {
            get { return value2; }
        }
    }
}
