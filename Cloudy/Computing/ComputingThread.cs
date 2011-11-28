using System;
using System.Threading;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing
{
    public class ComputingThread : IEnvironment
    {
        private readonly Action<IEnvironment> threadRun;

        public ComputingThread()
        {
            // Do nothing.
        }

        public ComputingThread(Action<IEnvironment> threadRun)
            : this()
        {
            this.threadRun = threadRun;
        }

        protected virtual void Run(IEnvironment environment)
        {
            threadRun(environment);
        }

        protected virtual void Run()
        {
            Run(this);
        }

        internal void Run(Guid threadId)
        {
            this.ThreadId = threadId;
            Run();
        }

        #region Implementation of IEnvironment

        public Guid ThreadId { get; private set; }

        #endregion
    }
}
