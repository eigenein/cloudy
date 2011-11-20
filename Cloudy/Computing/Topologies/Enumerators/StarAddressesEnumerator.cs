using System;
using System.Collections;
using System.Collections.Generic;
using Cloudy.Computing.Topologies.Structures;

namespace Cloudy.Computing.Topologies.Enumerators
{
    public class StarAddressesEnumerator : IEnumerator<ThreadAddress>
    {
        private readonly int size;

        private int current;

        public StarAddressesEnumerator(int size)
        {
            this.size = size;
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        #region Implementation of IEnumerator

        public bool MoveNext()
        {
            return current++ < size;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public ThreadAddress Current
        {
            get { return new ThreadAddress(current); }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            // Do nothing.
        }
    }
}
