using System;

namespace Cloudy.Structures
{
    public class Tuple<T1, T2>
    {
        private readonly T1 item1;

        private readonly T2 item2;

        public Tuple(T1 item1, T2 item2)
        {
            this.item1 = item1;
            this.item2 = item2;
        }

        public T1 Item1
        {
            get { return item1; }
        }

        public T2 Item2
        {
            get { return item2; }
        }
    }
}
