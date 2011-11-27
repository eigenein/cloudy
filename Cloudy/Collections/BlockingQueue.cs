﻿using System;
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
        private readonly Queue<T> queue = new Queue<T>();

        private readonly object queueLocker = new object();

        private readonly AutoResetEvent queueWaitHandle = new AutoResetEvent(false);

        /// <summary>
        /// Adds the object to the end of the queue.
        /// </summary>
        public void Enqueue(T item)
        {
            lock (queueLocker)
            {
                queue.Enqueue(item);
                queueWaitHandle.Set();
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
                lock (queueLocker)
                {
                    if (queue.Count != 0)
                    {
                        return queue.Dequeue();
                    }
                }
                if (!queueWaitHandle.WaitOne(timeout))
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
                lock (queueLocker)
                {
                    return queue.Count;
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
                queueWaitHandle.Close();
            }
        }
    }
}
