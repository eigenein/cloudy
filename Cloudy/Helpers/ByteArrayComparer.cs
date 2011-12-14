using System;
using System.Collections.Generic;
using System.Linq;

namespace Cloudy.Helpers
{
    public class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        private static readonly ByteArrayComparer instance = new ByteArrayComparer();

        public static ByteArrayComparer Instance
        {
            get { return instance; }
        }

        #region Implementation of IEqualityComparer<byte[]>

        public bool Equals(byte[] left, byte[] right)
        {
            if (left == null || right == null)
            {
                return left == right;
            }
            return left.SequenceEqual(right);
        }
        public int GetHashCode(byte[] key)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            return key.Sum(b => b);
        }

        #endregion
    }
}
