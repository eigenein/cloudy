using System;
using System.Collections.Generic;
using System.Linq;

namespace Cloudy.Helpers
{
    /// <summary>
    /// Represents the thread-safe memory storage for nodes.
    /// </summary>
    /// <typeparam name="TValue">The type of a value.</typeparam>
    public class MemoryStorage<TValue>
    {
        private readonly object syncronizationRoot = new object();

        private readonly Dictionary<byte[], Dictionary<string, TValue>> cache =
            new Dictionary<byte[], Dictionary<string, TValue>>(ByteArrayComparer.Instance);

        /// <summary>
        /// Adds the value to the storage.
        /// </summary>
        /// <param name="namespace">The namespace for the key.</param>
        /// <param name="key">The key reference.</param>
        /// <param name="value">The value.</param>
        public void Add(byte[] @namespace, string key, TValue value)
        {
            lock (syncronizationRoot)
            {
                Dictionary<string, TValue> subCache;
                if (!cache.TryGetValue(@namespace, out subCache))
                {
                    subCache = new Dictionary<string, TValue>();
                    cache.Add(@namespace, subCache);
                }
                subCache[key] = value;
            }
        }

        public bool TryGetValue(byte[] @namespace, string key, out TValue value)
        {
            lock (syncronizationRoot)
            {
                Dictionary<string, TValue> subCache;
                if (cache.TryGetValue(@namespace, out subCache) &&
                    subCache.TryGetValue(key, out value))
                {
                    return true;
                }
                value = default(TValue);
                return false;
            }
        }

        /// <summary>
        /// Removes values that satisfy the <paramref name="conditionPredicate"/>.
        /// </summary>
        public void CleanUp(Func<TValue, bool> conditionPredicate)
        {
            foreach (Dictionary<string, TValue> subCache in cache.Values)
            {
                IEnumerable<string> keysToRemove = subCache
                    .Where(pair => conditionPredicate(pair.Value))
                    .Select(pair => pair.Key).ToList();
                foreach (string key in keysToRemove)
                {
                    subCache.Remove(key);
                }
            }
        }
    }
}
