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
        private readonly Action<IEnvironment> run;

        public ComputingThread(Action<IEnvironment> run)
        {
            this.run = run;
        }

        #region Implementation of IComputingThread

        public void Run(IEnvironment environment)
        {
            run(environment);
        }

        #endregion
    }
}
