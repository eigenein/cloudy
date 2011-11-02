using System;
using System.Threading;

namespace Cloudy.Messaging.Structures
{
    /// <summary>
    /// Represents a result of a messaging asynchronous operation.
    /// </summary>
    public class MessagingAsyncResult : IAsyncResult, IDisposable
    {
        private readonly object state;

        private readonly AsyncCallback callback;

        private ManualResetEvent waitHandle = new ManualResetEvent(false);

        internal MessagingAsyncResult(AsyncCallback callback, object state)
        {
            this.state = state;
            this.callback = callback;
        }

        internal void SetCompleted()
        {
            ManualResetEvent waitHandleInstance = waitHandle;
            if (waitHandleInstance == null)
            {
                return;
            }
            waitHandleInstance.Set();
            if (callback != null)
            {
                callback(this);
            }
        }

        public void EndInvoke(TimeSpan timeout)
        {
            ManualResetEvent waitHandleInstance = waitHandle;
            if (waitHandleInstance == null)
            {
                return;
            }
            waitHandle = null;
            waitHandleInstance.WaitOne(timeout);
            waitHandleInstance.Close();
        }

        #region Implementation of IAsyncResult

        /// <summary>
        /// Gets an indication whether the asynchronous operation has completed.
        /// </summary>
        /// <returns>
        /// true if the operation is complete; otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public bool IsCompleted
        {
            get { return waitHandle.WaitOne(-1, true); }
        }

        /// <summary>
        /// Gets a <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an asynchronous operation to complete.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an asynchronous operation to complete.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public WaitHandle AsyncWaitHandle
        {
            get { return waitHandle; }
        }

        /// <summary>
        /// Gets a user-defined object that qualifies or contains information about an asynchronous operation.
        /// </summary>
        /// <returns>
        /// A user-defined object that qualifies or contains information about an asynchronous operation.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object AsyncState
        {
            get { return state; }
        }

        /// <summary>
        /// Gets an indication of whether the asynchronous operation completed synchronously.
        /// </summary>
        /// <returns>
        /// true if the asynchronous operation completed synchronously; otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public bool CompletedSynchronously
        {
            get { return false; }
        }

        #endregion

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
                ManualResetEvent waitHandleInstance = waitHandle;
                if (waitHandleInstance == null)
                {
                    return;
                }
                waitHandle = null;
                waitHandleInstance.Close();
            }
        }
    }

    /// <summary>
    /// Represents a result of a messaging asynchronous operation.
    /// </summary>
    public class MessagingAsyncResult<T> : MessagingAsyncResult
    {
        internal MessagingAsyncResult(AsyncCallback callback, object state)
            : base(callback, state)
        {
            // Do nothing.
        }

        public T Result { get; internal set; }

        public T EndInvokeWithResult(TimeSpan timeout)
        {
            EndInvoke(timeout);
            return Result;
        }
    }
}
