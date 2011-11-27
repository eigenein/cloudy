using System;
using System.Collections.Generic;
using System.Threading;
using Cloudy.Helpers;

namespace Cloudy.Collections
{
    public class BlockingMultiDictionary<TKey, TValue> : IDisposable
    {
        private readonly object locker = new object();

        private readonly AutoResetEvent waitHandle = new AutoResetEvent(false);

        private readonly Dictionary<TKey, Queue<TValue>> dictionary =
            new Dictionary<TKey, Queue<TValue>>();

        public void Enqueue(TKey key, TValue value)
        {
            lock (locker)
            {
                Queue<TValue> queue;
                if (!dictionary.TryGetValue(key, out queue))
                {
                    queue = dictionary[key] = new Queue<TValue>();
                }
                queue.Enqueue(value);
                waitHandle.Set();
            }
        }

        public TValue Dequeue(TKey key)
        {
            return Dequeue(key, TimeSpanExtensions.Infinite);
        }

        public TValue Dequeue(TKey key, TimeSpan timeout)
        {
            while (true)
            {
                lock (locker)
                {
                    Queue<TValue> queue;
                    if (dictionary.TryGetValue(key, out queue) && queue.Count != 0)
                    {
                        return queue.Dequeue();
                    }
                }
                if (!waitHandle.WaitOne(timeout))
                {
                    throw new TimeoutException(
                        "No elements were enqueued during the last timeout interval.");
                }
            }
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
                waitHandle.Close();
            }
        }
    }
}
