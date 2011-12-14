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
        private readonly byte[] rank;

        private readonly IInternalEnvironment environment;

        private readonly Func<IComputingThread> createThread;

        private Thread thread;

        public event ParameterizedEventHandler<byte[], Exception> ThreadFailed;

        public event ParameterizedEventHandler<byte[]> ThreadCompleted;

        public event ParameterizedEventHandler<byte[]> ThreadStopped;

        public ComputingThreadWrapper(byte[] rank, IInternalEnvironment environment,
            Func<IComputingThread> createThread)
        {
            this.rank = rank;
            this.environment = environment;
            this.createThread = createThread;
        }

        public byte[] Rank
        {
            get { return rank; }
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
                ThreadStopped(this, new EventArgs<byte[]>(rank));
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
                    ThreadCompleted(this, new EventArgs<byte[]>(rank));
                }
            }
            catch (Exception ex)
            {
                if (ThreadFailed != null)
                {
                    ThreadFailed(this, new EventArgs<byte[], Exception>(rank, ex));
                }
            }
            OnThreadStopped();
        }
    }
}
