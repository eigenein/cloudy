using System;
using System.Collections.Generic;

namespace Cloudy.Collections
{
    public class CachedFactory<TKey, TValue> 
        where TValue : new()
    {
        private readonly object synchronizationRoot = new object();

        private readonly Dictionary<TKey, TValue> cache =
            new Dictionary<TKey, TValue>();

        public TValue this[TKey key]
        {
            get
            {
                lock (synchronizationRoot)
                {
                    TValue value;
                    if (!cache.TryGetValue(key, out value))
                    {
                        value = cache[key] = new TValue();
                    }
                    return value;
                }
            }
        }
    }
}
