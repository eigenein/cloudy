using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Structures.Values;
using Cloudy.Helpers;
using Cloudy.Messaging.Interfaces;

namespace Cloudy.Computing
{
    public abstract class AbstractSlaveNode : AbstractNode
    {
        private readonly IPAddress localAddress;

        private readonly Func<ComputingThread> createThread;

        private readonly Dictionary<Guid, Thread> threads =
            new Dictionary<Guid, Thread>();

        private IPEndPoint masterEndPoint;

        private SlaveState state;

        protected AbstractSlaveNode(int port, IPAddress localAddress, 
            Func<ComputingThread> createThread)
            : base(port)
        {
            this.localAddress = localAddress;
            this.createThread = createThread;
            AddHandler(Tags.Bye, OnBye);
            AddHandler(Tags.StartThread, OnStartThread);
            AddHandler(Tags.StopThread, OnStopThread);
            State = SlaveState.NotJoined;
        }

        public Guid? SlaveId { get; protected set; }

        public abstract int SlotsCount { get; }

        public IPEndPoint LocalEndPoint
        {
            get { return new IPEndPoint(localAddress, Port); }
        }

        public IPEndPoint ExternalEndPoint { get; private set; }

        public SlaveState State
        {
            get { return state; }
            private set
            {
                state = value;
                if (StateChanged != null)
                {
                    StateChanged(this, new EventArgs<SlaveState>(state));
                }
            }
        }

        public event ParametrizedEventHandler<IPEndPoint, Guid> Joined;

        public event ParametrizedEventHandler<SlaveState> StateChanged;

        public event ParametrizedEventHandler<Guid> ThreadStarted;

        public event ParametrizedEventHandler<Guid> ThreadStopped;

        public event ParametrizedEventHandler<Exception> ExceptionUnhandled;

        public bool Join(IPEndPoint endPoint)
        {
            JoinRequestValue request = 
                new JoinRequestValue
                {
                    LocalEndPoint = new EndPointValue { Value = LocalEndPoint },
                    SlaveId = SlaveId,
                    SlotsCount = SlotsCount
                };
            Send(endPoint, request, Tags.JoinRequest);
            JoinResponseValue response = ReceiveFrom<JoinResponseValue>(endPoint);
            this.SlaveId = response.SlaveId;
            this.ExternalEndPoint = response.ExternalEndPoint.Value;
            this.masterEndPoint = endPoint;
            State = SlaveState.Joined;
            if (Joined != null)
            {
                Joined(this, new EventArgs<IPEndPoint, Guid>(
                    response.ExternalEndPoint.Value, response.SlaveId));
            }
            return true;
        }

        private void AbortThread(Guid threadId, Thread thread)
        {
            if (thread.IsAlive)
            {
                SendAsync(masterEndPoint, new GuidValue { Value = threadId }, Tags.ThreadFailed);
                thread.Abort();
            }
        }

        private bool OnStartThread(IPEndPoint remoteEndPoint, IMessage message)
        {
            Guid threadId = message.Get<GuidValue>().Value;
            Thread thread;
            if (threads.TryGetValue(threadId, out thread))
            {
                AbortThread(threadId, thread);
            }
            thread = threads[threadId] = new Thread(RunClientCode);
            thread.Start(threadId);
            if (ThreadStarted != null)
            {
                ThreadStarted(this, new EventArgs<Guid>(threadId));
            }
            return true;
        }

        private void RunClientCode(object o)
        {
            Guid threadId = (Guid)o;
            try
            {
                createThread().Run(threadId);
                Send(masterEndPoint, new GuidValue { Value = threadId }, Tags.ThreadCompleted);
            }
            catch (Exception ex)
            {
                if (ExceptionUnhandled != null)
                {
                    ExceptionUnhandled(this, new EventArgs<Exception>(ex));
                }
                Send(masterEndPoint, new GuidValue { Value = threadId }, Tags.ThreadFailed);
            }
            OnThreadStopped(threadId);
        }

        private bool OnStopThread(IPEndPoint remoteEndPoint, IMessage message)
        {
            Guid threadId = message.Get<GuidValue>().Value;
            Thread thread;
            if (threads.TryGetValue(threadId, out thread) && thread.IsAlive)
            {
                thread.Abort();
                OnThreadStopped(threadId);
            }
            return true;
        }

        private void OnThreadStopped(Guid threadId)
        {
            if (ThreadStopped != null)
            {
                ThreadStopped(this, new EventArgs<Guid>(threadId));
            }
        }

        public void Close()
        {
            foreach (KeyValuePair<Guid, Thread> pair in threads)
            {
                AbortThread(pair.Key, pair.Value);
            }
            SendAsync(masterEndPoint, EmptyValue.Instance, Tags.Bye);
            State = SlaveState.Left;
        }

        private bool OnBye(IPEndPoint remoteEndPoint, IMessage message)
        {
            Close();
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
            base.Dispose(disposing);
        }
    }
}
