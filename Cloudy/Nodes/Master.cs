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
            // TODO
        }

        #endregion
    }
}
