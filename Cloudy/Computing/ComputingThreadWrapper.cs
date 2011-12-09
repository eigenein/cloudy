using System;
using System.Threading;
using Cloudy.Computing.Interfaces;
using Cloudy.Helpers;

namespace Cloudy.Computing
{
    /// <summary>
    /// Takes care about (re)starting of an underlying thread object.
    /// </summary>
    internal class ComputingThreadWrapper
    {
        private readonly Guid threadId;

        private readonly IInternalEnvironment environment;

        private readonly Func<IComputingThread> createThread;

        private Thread thread;

        public event ParametrizedEventHandler<Exception> ThreadFailed;

        public event EventHandler ThreadCompleted;

        public event EventHandler ThreadStopped;

        public ComputingThreadWrapper(Guid threadId, IInternalEnvironment environment,
            Func<IComputingThread> createThread)
        {
            this.threadId = threadId;
            this.environment = environment;
            this.createThread = createThread;
        }

        public Guid ThreadId
        {
            get { return threadId; }
        }

        public IInternalEnvironment Environment
        {
            get { return environment; }
        }

        public void Abort()
        {
            if (thread != null && thread.IsAlive)
            {
                thread.Abort();
                OnThreadStopped();
            }
        }

        public void Restart()
        {
            Abort();
            thread = new Thread(Run);
            thread.Start(null);
        }

        private void OnThreadStopped()
        {
            thread = null;
            if (ThreadStopped != null)
            {
                ThreadStopped(this, new EventArgs());
            }
        }

        private void Run(object state)
        {
            IComputingThread computingThread = createThread();
            try
            {
                computingThread.Run(environment);
                if (ThreadCompleted != null)
                {
                    ThreadCompleted(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                if (ThreadFailed != null)
                {
                    ThreadFailed(this, new EventArgs<Exception>(ex));
                }
            }
            OnThreadStopped();
        }
    }
}
