using System;

namespace Cloudy.Nodes
{
    /// <summary>
    /// Represents an abstract node and implements common logic for both
    /// master and slave.
    /// </summary>
    public abstract class Node : IDisposable
    {
        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
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
                // Do nothing.
            }
        }
    }
}
