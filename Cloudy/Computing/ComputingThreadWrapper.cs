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
        private readonly IInternalEnvironment environment;

        private readonly Func<IComputingThread> createThread;

        private Thread thread;

        public event ParameterizedEventHandler<byte[], Exception> ThreadFailed;

        public event ParameterizedEventHandler<byte[]> ThreadCompleted;

        public event ParameterizedEventHandler<byte[]> ThreadStopped;

        public ComputingThreadWrapper(IInternalEnvironment environment,
            Func<IComputingThread> createThread)
        {
            this.environment = environment;
            this.createThread = createThread;
        }

        public byte[] Rank
        {
            get { return environment.RawRank; }
            set { environment.RawRank = value; }
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
            environment.ResetTime();
            thread = new Thread(Run);
            thread.Start(null);
        }

        private void OnThreadStopped()
        {
            thread = null;
            environment.CleanUp();
            if (ThreadStopped != null)
            {
                ThreadStopped(this, new EventArgs<byte[]>(Rank));
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
                    ThreadCompleted(this, new EventArgs<byte[]>(Rank));
                }
            }
            catch (Exception ex)
            {
                if (ThreadFailed != null)
                {
                    ThreadFailed(this, new EventArgs<byte[], Exception>(Rank, ex));
                }
            }
            OnThreadStopped();
        }
    }
}
