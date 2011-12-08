using System;
using System.Collections.Generic;
using Cloudy.Helpers;

namespace Cloudy.Collections
{
    public class BlockingFilteredQueue<T> : BlockingQueue<T>
    {
        public T Dequeue(Func<T, bool> filter, TimeSpan timeout)
        {
            // Check if there is already an appropriate item.
            lock (SynchronizationRoot)
            {
                foreach (LinkedListNode<T> node in List.EnumerateNodes())
                {
                    if (filter(node.Value))
                    {
                        List.Remove(node);
                        return node.Value;
                    }
                }
            }
            // No, then wait for a newly enqueued item.
            while (true)
            {
                lock (SynchronizationRoot)
                {
                    LinkedListNode<T> node = List.First;
                    if (node != null && filter(node.Value))
                    {
                        List.Remove(node);
                        return node.Value;
                    }
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
    }
}
