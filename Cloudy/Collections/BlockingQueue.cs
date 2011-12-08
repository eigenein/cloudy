using System;
using System.Collections.Generic;
using System.Threading;
using Cloudy.Helpers;

namespace Cloudy.Collections
{
    /// <summary>
    /// Implements the producer-consumer pattern.
    /// </summary>
    public class BlockingQueue<T> : IDisposable
    {
        protected readonly LinkedList<T> List = new LinkedList<T>();

        protected readonly object SynchronizationRoot = new object();

        protected readonly AutoResetEvent WaitHandle = new AutoResetEvent(false);

        /// <summary>
        /// Adds the object to the end of the queue.
        /// </summary>
        public void Enqueue(T item)
        {
            lock (SynchronizationRoot)
            {
                List.AddLast(item);
                WaitHandle.Set();
            }
        }

        /// <summary>
        /// Removes and returns the object from the beginning of the queue.
        /// Waits for the object if the queue is empty.
        /// </summary>
        public T Dequeue()
        {
            return Dequeue(TimeSpanExtensions.Infinite);
        }

        /// <summary>
        /// Removes and returns the object from the beginning of the queue.
        /// Waits for the object if the queue is empty.
        /// </summary>
        public T Dequeue(TimeSpan timeout)
        {
            while (true)
            {
                lock (SynchronizationRoot)
                {
                    LinkedListNode<T> node = List.First;
                    if (node != null)
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

        /// <summary>
        /// Gets the count of elements contained in the queue.
        /// </summary>
        public int Count
        {
            get
            {
                lock (SynchronizationRoot)
                {
                    return List.Count;
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
                WaitHandle.Close();
            }
        }
    }
}
