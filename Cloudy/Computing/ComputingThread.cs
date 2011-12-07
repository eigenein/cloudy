using System;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing
{
    /// <summary>
    /// Default implementation of the <see cref="IComputingThread"/> interface
    /// that accepts an action delegate.
    /// </summary>
    public class ComputingThread : IComputingThread
    {
        private readonly Action<Guid, IEnvironment> run;

        public ComputingThread(Action<Guid, IEnvironment> run)
        {
            this.run = run;
        }

        #region Implementation of IComputingThread

        public void Run(Guid threadId, IEnvironment environment)
        {
            run(threadId, environment);
        }

        #endregion
    }
}
