using System;
using System.Threading;
using Cloudy.Helpers;

namespace Cloudy.Computing
{
    internal class ComputingThreadWrapper
    {
        private readonly Guid threadId;

        private readonly Func<ComputingThread> createThread;

        private Thread thread;

        public event ParametrizedEventHandler<Exception> ThreadFailed;

        public event EventHandler ThreadCompleted;

        public event EventHandler ThreadStopped;

        public ComputingThreadWrapper(Guid threadId, Func<ComputingThread> createThread)
        {
            this.threadId = threadId;
            this.createThread = createThread;
        }

        public Guid ThreadId
        {
            get { return threadId; }
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
            ComputingThread computingThread = createThread();
            try
            {
                computingThread.Run(threadId);
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
