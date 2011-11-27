using System;
using System.Net;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Structures;
using Cloudy.Computing.Structures.Values;
using Cloudy.Helpers;
using Cloudy.Messaging.Interfaces;

namespace Cloudy.Computing
{
    public abstract class AbstractSlaveNode : AbstractNode
    {
        private readonly IPAddress localAddress;

        private IPEndPoint masterEndPoint;

        private SlaveState state;

        protected AbstractSlaveNode(int port, IPAddress localAddress)
            : base(port)
        {
            this.localAddress = localAddress;
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
            GuidValue threadId = message.Get<GuidValue>();
            if (ThreadStarted != null)
            {
                ThreadStarted(this, new EventArgs<Guid>(threadId.Value));
            }
            return true;
        }

        private bool OnStopThread(IPEndPoint remoteEndPoint, IMessage message)
        {
            GuidValue threadId = message.Get<GuidValue>();
            if (ThreadStopped != null)
            {
                ThreadStopped(this, new EventArgs<Guid>(threadId.Value));
            }
            return true;
        }

        public void Close()
        {
            // TODO: Stop all threads if any.
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
