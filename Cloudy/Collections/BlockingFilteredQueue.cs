using System;
using System.Collections.Generic;
using Cloudy.Helpers;

namespace Cloudy.Collections
{
    public class BlockingFilteredQueue<T> : BlockingQueue<T>
    {
        public T Dequeue(Func<T, bool> filter, TimeSpan timeout)
        {
            while (true)
            {
                T value;
                if (TrySearch(filter, out value))
                {
                    return value;
                }
                if (!WaitHandle.WaitOne(timeout))
                {
                    throw new TimeoutException(
                        "No elements were enqueued during the last timeout interval.");
                }
            }
        }

        public T Dequeue(Func<T, bool> filter)
        {
            return Dequeue(filter, TimeSpanExtensions.Infinite);
        }

        private bool TrySearch(Func<T, bool> filter, out T value)
        {
            lock (SynchronizationRoot)
            {
                foreach (LinkedListNode<T> node in List.EnumerateNodes())
                {
                    if (filter(node.Value))
                    {
                        value = node.Value;
                        List.Remove(node);
                        return true;
                    }
                }
                value = default(T);
                return false;
            }
        }
    }
}
