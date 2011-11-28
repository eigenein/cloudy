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

        private readonly Dictionary<Guid, ComputingThreadWrapper> threads =
            new Dictionary<Guid, ComputingThreadWrapper>();

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

        private bool OnStartThread(IPEndPoint remoteEndPoint, IMessage message)
        {
            Guid threadId = message.Get<GuidValue>().Value;
            ComputingThreadWrapper thread;
            if (!threads.TryGetValue(threadId, out thread))
            {
                thread = threads[threadId] = new ComputingThreadWrapper(
                    threadId, createThread);
                thread.ThreadCompleted += OnThreadCompleted;
                thread.ThreadFailed += OnThreadFailed;
                thread.ThreadStopped += OnThreadStopped;
            }
            thread.Restart();
            if (ThreadStarted != null)
            {
                ThreadStarted(this, new EventArgs<Guid>(threadId));
            }
            return true;
        }

        private void OnThreadCompleted(object sender, EventArgs e)
        {
            Guid threadId = ((ComputingThreadWrapper)sender).ThreadId;
            Send(masterEndPoint, new GuidValue { Value = threadId }, Tags.ThreadCompleted);
        }

        private void OnThreadFailed(object sender, EventArgs<Exception> e)
        {
            Guid threadId = ((ComputingThreadWrapper)sender).ThreadId;
            Send(masterEndPoint, new GuidValue { Value = threadId }, Tags.ThreadFailed);
            if (e.Value != null && ExceptionUnhandled != null)
            {
                ExceptionUnhandled(this, new EventArgs<Exception>(e.Value));
            }
        }

        private bool OnStopThread(IPEndPoint remoteEndPoint, IMessage message)
        {
            Guid threadId = message.Get<GuidValue>().Value;
            ComputingThreadWrapper thread;
            if (threads.TryGetValue(threadId, out thread))
            {
                thread.Abort();
            }
            return true;
        }

        private void OnThreadStopped(object sender, EventArgs e)
        {
            OnThreadStopped(((ComputingThreadWrapper)sender).ThreadId);
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
            foreach (ComputingThreadWrapper thread in threads.Values)
            {
                thread.Abort();
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
