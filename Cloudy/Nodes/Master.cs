using System;

namespace Cloudy.Nodes
{
    /// <summary>
    /// Represents an abstract master.
    /// </summary>
    public abstract class Master : IDisposable
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
