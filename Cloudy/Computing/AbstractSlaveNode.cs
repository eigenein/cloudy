using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Structures.Values;
using Cloudy.Helpers;
using Cloudy.Messaging.Interfaces;
using Cloudy.Structures;

namespace Cloudy.Computing
{
    public abstract class AbstractSlaveNode : AbstractNode, IEnvironmentTransport
    {
        private readonly IPAddress localAddress;

        private IPEndPoint masterEndPoint;

        private SlaveState state;

        /// <summary>
        /// Use this lock every time you need a bi-directional conversation
        /// with the Master using both Send and Receive methods. One-directional
        /// replies and notifications don't need this lock.
        /// </summary>
        private readonly object masterConversationLock = new object();

        /// <summary>
        /// Holds local threads.
        /// </summary>
        private readonly Dictionary<Guid, ComputingThreadWrapper> threads =
            new Dictionary<Guid, ComputingThreadWrapper>();

        protected AbstractSlaveNode(int port, IPAddress localAddress)
            : base(port)
        {
            this.localAddress = localAddress;
            AddHandler(Tags.Bye, OnBye);
            AddHandler(Tags.StartThread, OnStartThread);
            AddHandler(Tags.StopThread, OnStopThread);
            AddHandler(Tags.EnvironmentOperation, OnEnvironmentOperation);
            AddHandler(Tags.SignedPing, OnSignedPing);
            AddHandler(Tags.SignedPingRequest, OnSignedPingRequest);
            State = SlaveState.NotJoined;
            MaxPortScanOffset = 3;
        }

        public Guid? SlaveId { get; protected set; }

        /// <summary>
        /// Gets the available count of threads running on this slave.
        /// </summary>
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

        /// <summary>
        /// Specifies a maximum offset from the current port number
        /// when performing port scanning during UDP hole punching.
        /// </summary>
        public int MaxPortScanOffset { get; set; }

        public event ParameterizedEventHandler<IPEndPoint, Guid> Joined;

        public event ParameterizedEventHandler<SlaveState> StateChanged;

        public event ParameterizedEventHandler<Guid> ThreadStarted;

        public event ParameterizedEventHandler<Guid> ThreadStopped;

        public event ParameterizedEventHandler<Exception> ExceptionUnhandled;

        public event ParameterizedEventHandler<Guid, IPEndPoint> CreatingWormHole;

        public event ParameterizedEventHandler<IPEndPoint, IPEndPoint> PortScanning;

        public event ParameterizedEventHandler<IPEndPoint, IPEndPoint> SignedPingRequested;

        public event ParameterizedEventHandler<Guid, IPEndPoint> EndPointResolved;

        /// <summary>
        /// Creates a thread within this slave node.
        /// </summary>
        protected abstract IComputingThread CreateThread();

        public bool Join(IPEndPoint endPoint)
        {
            JoinRequestValue request = 
                new JoinRequestValue
                {
                    LocalEndPoint = new EndPointValue { Value = LocalEndPoint },
                    SlaveId = SlaveId,
                    SlotsCount = SlotsCount
                };
            JoinResponseValue response;
            lock (masterConversationLock)
            {
                Send(endPoint, request, Tags.JoinRequest);
                response = ReceiveFrom<JoinResponseValue>(endPoint);
            }
            SlaveId = response.SlaveId;
            ExternalEndPoint = response.ExternalEndPoint.Value;
            masterEndPoint = endPoint;
            State = SlaveState.Joined;
            if (Joined != null)
            {
                Joined(this, new EventArgs<IPEndPoint, Guid>(
                    response.ExternalEndPoint.Value, response.SlaveId));
            }
            return true;
        }

        private void OnStartThread(IPEndPoint remoteEndPoint, IMessage message)
        {
            Guid threadId = message.Get<GuidValue>().Value;
            ComputingThreadWrapper thread;
            if (!threads.TryGetValue(threadId, out thread))
            {
                thread = threads[threadId] = new ComputingThreadWrapper(
                    threadId, new Environment(this, threadId), CreateThread);
                thread.ThreadCompleted += OnThreadCompleted;
                thread.ThreadFailed += OnThreadFailed;
                thread.ThreadStopped += OnThreadStopped;
            }
            thread.Restart();
            if (ThreadStarted != null)
            {
                ThreadStarted(this, new EventArgs<Guid>(threadId));
            }
        }

        private void OnThreadCompleted(object sender, EventArgs e)
        {
            Guid threadId = ((ComputingThreadWrapper)sender).ThreadId;
            Send(masterEndPoint, new GuidValue { Value = threadId }, Tags.ThreadCompleted);
        }

        /// <summary>
        /// Called when the thread is abnormally terminated.
        /// </summary>
        /// <param name="e">Contains an exception object if any have been thrown.</param>
        private void OnThreadFailed(object sender, EventArgs<Exception> e)
        {
            Guid threadId = ((ComputingThreadWrapper)sender).ThreadId;
            Send(masterEndPoint, new GuidValue { Value = threadId }, Tags.ThreadFailed);
            if (e.Value != null && ExceptionUnhandled != null)
            {
                ExceptionUnhandled(this, new EventArgs<Exception>(e.Value));
            }
        }

        private void OnStopThread(IPEndPoint remoteEndPoint, IMessage message)
        {
            Guid threadId = message.Get<GuidValue>().Value;
            ComputingThreadWrapper thread;
            if (threads.TryGetValue(threadId, out thread))
            {
                thread.Abort();
            }
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

        private void OnEnvironmentOperation(IPEndPoint remoteEndPoint, IMessage message)
        {
            EnvironmentOperationValue value = message.Get<EnvironmentOperationValue>();
            HashSet<Guid> nextRecipientsIds = new HashSet<Guid>();
            foreach (Guid recipientId in value.RecipientsIds)
            {
                ComputingThreadWrapper thread;
                if (threads.TryGetValue(recipientId, out thread))
                {
                    thread.Environment.NotifyValueReceived(value);
                }
                else
                {
                    nextRecipientsIds.Add(recipientId);
                }
            }
            value.RecipientsIds = nextRecipientsIds;
            SendEnvironmentOperation(value); // Send further to the recipients left.
        }

        private void OnEndPointResolved(Guid threadId, IPEndPoint endPoint)
        {
            if (EndPointResolved != null)
            {
                EndPointResolved(this, new EventArgs<Guid, IPEndPoint>(
                    threadId, endPoint));
            }
        }

        private void OnSignedPing(IPEndPoint remoteEndPoint, IMessage message)
        {
            Guid threadId = message.Get<GuidValue>().Value;
            endPoints[threadId] = remoteEndPoint;
            OnEndPointResolved(threadId, remoteEndPoint);
        }

        private void OnSignedPingRequest(IPEndPoint remoteEndPoint, IMessage message)
        {
            SignedPingRequest request = message.Get<SignedPingRequest>();
            ThreadPool.UnsafeQueueUserWorkItem(
                o => MakeSignedPingRequest(remoteEndPoint, request), null);
        }

        private void MakeSignedPingRequest(IPEndPoint remoteEndPoint, SignedPingRequest request)
        {
            SignedPingResponse response = new SignedPingResponse();
            response.SenderExternalEndPoint = request.SenderExternalEndPoint;
            IPEndPoint senderLocalEndPoint = request.SenderLocalEndPoint.Value;
            IPEndPoint senderExternalEndPoint = request.SenderExternalEndPoint.Value;
            if (SignedPingRequested != null)
            {
                SignedPingRequested(this, new EventArgs<IPEndPoint, IPEndPoint>(
                    senderLocalEndPoint, senderExternalEndPoint));
            }
            if (MakeSignedPing(request.TargetId, senderLocalEndPoint))
            {
                endPoints[request.SenderId] = senderLocalEndPoint;
                response.Success = true;
            }
            else if (MakeSignedPing(request.TargetId, senderExternalEndPoint))
            {
                endPoints[request.SenderId] = senderExternalEndPoint;
                response.Success = true;
            }
            SendAsync(remoteEndPoint, response, Tags.SignedPingResponse);
        }

        private bool MakeSignedPing(Guid currentThreadId, IPEndPoint targetEndPoint)
        {
            GuidValue value = new GuidValue();
            value.Value = currentThreadId;
            try
            {
                Send(targetEndPoint, value, Tags.SignedPing);
                return true;
            }
            catch (TimeoutException)
            {
                return false;
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

        private void OnBye(IPEndPoint remoteEndPoint, IMessage message)
        {
            Close();
        }

        #region Implementation of IEnvironmentTransport

        void IEnvironmentTransport.Send(EnvironmentOperationValue operationValue)
        {
            SendEnvironmentOperation(operationValue);
        }

        ICollection<Guid> IEnvironmentTransport.ResolveId(Guid threadId, Guid id)
        {
            return ResolveRecipient(threadId, id);
        }

        #endregion

        #region Environment Operations

        /// <summary>
        /// Locks the routes cache.
        /// </summary>
        private readonly object routeSynchronizationRoot = new object();

        /// <summary>
        /// Caches the next step to route a message to a destination.
        /// </summary>
        private readonly Dictionary<Guid, Dictionary<Guid, Guid>> routes =
            new Dictionary<Guid, Dictionary<Guid, Guid>>();

        private readonly object recipientsSynchronizationRoot = new object();

        /// <summary>
        /// Caches resolution of recipients and shortcuts to the final IDs.
        /// </summary>
        private readonly Dictionary<Guid, Dictionary<Guid, ICollection<Guid>>> recipients =
            new Dictionary<Guid, Dictionary<Guid, ICollection<Guid>>>();

        private readonly object endPointsSynchronizationRoot = new object();

        /// <summary>
        /// Caches endpoints of the threads.
        /// </summary>
        private readonly Dictionary<Guid, IPEndPoint> endPoints =
            new Dictionary<Guid, IPEndPoint>();

        private Guid ResolveRoute(Guid currentThreadId, Guid recipientId)
        {
            lock (routeSynchronizationRoot)
            {
                Dictionary<Guid, Guid> subCache;
                if (!routes.TryGetValue(currentThreadId, out subCache))
                {
                    routes[currentThreadId] = subCache = new Dictionary<Guid, Guid>();
                }
                Guid nextRecipientId;
                if (subCache.TryGetValue(recipientId, out nextRecipientId))
                {
                    return nextRecipientId;
                }
                RouteRequestValue value = new RouteRequestValue();
                value.CurrentThreadId = currentThreadId;
                value.DestinationThreadId = recipientId;
                lock (masterConversationLock)
                {
                    Send(masterEndPoint, value, Tags.RouteRequest);
                    nextRecipientId = ReceiveFrom<GuidValue>(masterEndPoint).Value;
                }
                return subCache[recipientId] = nextRecipientId;
            }
        }

        private ICollection<Guid> ResolveRecipient(Guid currentThreadId, Guid recipientId)
        {
            if (recipientId == Guid.Empty)
            {
                // Loopback.
                return new[] { currentThreadId };
            }
            lock (recipientsSynchronizationRoot)
            {
                Dictionary<Guid, ICollection<Guid>> subCache;
                if (!recipients.TryGetValue(currentThreadId, out subCache))
                {
                    recipients[currentThreadId] = subCache = 
                        new Dictionary<Guid, ICollection<Guid>>();
                }
                ICollection<Guid> resolvedIds;
                if (subCache.TryGetValue(recipientId, out resolvedIds))
                {
                    return resolvedIds;
                }
                ResolveRecipientRequestValue value = new ResolveRecipientRequestValue();
                value.CurrentThreadId = currentThreadId;
                value.RecipientId = recipientId;
                ResolveRecipientResponseValue response;
                lock (masterConversationLock)
                {
                    Send(masterEndPoint, value, Tags.ResolveRecipientRequest);
                    response = ReceiveFrom<ResolveRecipientResponseValue>(masterEndPoint);
                }
                return subCache[recipientId] = response.ResolvedTo;
            }
        }

        private void SendEnvironmentOperation(EnvironmentOperationValue operationValue)
        {
            if (operationValue.RecipientsIds.Count == 0)
            {
                return;
            }
            ICollection<Guid> resolvedRecipientsIds = operationValue.RecipientsIds;
            if (operationValue.RecipientsResolved != true)
            {
                HashSet<Guid> resolvedRecipientsIdsSet = new HashSet<Guid>();
                foreach (Guid recipientId in operationValue.RecipientsIds)
                {
                    // First, we should resolve all the shortcuts.
                    resolvedRecipientsIdsSet.UnionWith(ResolveRecipient(
                        operationValue.SenderId, recipientId));
                }
                operationValue.RecipientsIds = resolvedRecipientsIds =
                    resolvedRecipientsIdsSet.ToList();
                operationValue.RecipientsResolved = true;
            }
            HashSet<Guid> nextThreadsIds = new HashSet<Guid>();
            foreach (Guid recipientId in resolvedRecipientsIds)
            {
                // Second, we should find all the routes.
                nextThreadsIds.Add(ResolveRoute(operationValue.SenderId,
                    recipientId));
            }
            foreach (Guid recipientId in nextThreadsIds)
            {
                // Third, we should send the value to each route.
                SendEnvironmentOperation(recipientId, operationValue);
            }
        }

        private IPEndPoint ResolveThreadToEndPoint(Guid currentThreadId, Guid threadId)
        {
            if (threadId == Guid.Empty)
            {
                // The loopback ID.
                return LocalEndPoint;
            }
            lock (endPointsSynchronizationRoot)
            {
                IPEndPoint targetEndPoint;
                if (endPoints.TryGetValue(threadId, out targetEndPoint))
                {
                    return targetEndPoint;
                }
                EndPointResponseValue response;
                lock (masterConversationLock)
                {
                    Send(masterEndPoint, new GuidValue { Value = threadId },
                        Tags.EndPointRequest);
                    response = ReceiveFrom<EndPointResponseValue>(masterEndPoint);
                }
                IPEndPoint succeededEndPoint;
                if (CreateWormhole(currentThreadId, threadId, response.LocalEndPoint.Value,
                    out succeededEndPoint) ||
                    CreateWormhole(currentThreadId, threadId, response.ExternalEndPoint.Value,
                    out succeededEndPoint))
                {
                    OnEndPointResolved(threadId, succeededEndPoint);
                    return endPoints[threadId] = succeededEndPoint;
                }
                // TODO: Handle this case more smartly:
                // TODO: re-resolve, create a bridge through master etc ...
                throw new IOException(String.Format(
                    "Cannot create a wormhole to the thread {0} at {1}",
                    threadId, response));
            }
        }

        /// <summary>
        /// Performs the UDP hole punching.
        /// </summary>
        /// <returns>Whether the method call was succedded.</returns>
        private bool CreateWormhole(Guid currentThreadId, Guid targetThreadId,
            IPEndPoint targetEndPoint, out IPEndPoint succeededEndPoint)
        {
            succeededEndPoint = targetEndPoint;
            if (CreatingWormHole != null)
            {
                CreatingWormHole(this, new EventArgs<Guid, IPEndPoint>(
                    targetThreadId, targetEndPoint));
            }
            if (MakeSignedPing(currentThreadId, targetEndPoint))
            {
                // Hooray! We've established a connection!
                return true;
            }
            // No, then we should make request for the external ping.
            lock (masterConversationLock)
            {
                SignedPingRequest request = new SignedPingRequest();
                request.SenderId = currentThreadId;
                request.TargetId = targetThreadId;
                request.SenderLocalEndPoint.Value = LocalEndPoint;
                request.SenderExternalEndPoint.Value = ExternalEndPoint;
                Send(masterEndPoint, request, Tags.SignedPingRequest);
                SignedPingResponse response = ReceiveFrom<SignedPingResponse>(
                    masterEndPoint);
                return response.Success == true ||
                    DoPortScan(currentThreadId, targetEndPoint, out succeededEndPoint);
            }
        }

        /// <summary>
        /// Attempts to find the external port number of the node.
        /// </summary>
        /// <param name="currentThreadId">Current thread ID.</param>
        /// <param name="initialEndPoint">The endpoint to start with.</param>
        /// <param name="succeededEndPoint"></param>
        /// <returns>Whether a call was successful.</returns>
        private bool DoPortScan(Guid currentThreadId, IPEndPoint initialEndPoint,
            out IPEndPoint succeededEndPoint)
        {
            /*
             * We start with the initial endpoint because it's possible that
             * the other node cannot ping this node, but it has opened a hole
             * on its side.
             */
            foreach (IPEndPoint endPoint in initialEndPoint.GetPortScanEndPoints(MaxPortScanOffset))
            {
                if (PortScanning != null)
                {
                    PortScanning(this, new EventArgs<IPEndPoint, IPEndPoint>(
                        initialEndPoint, endPoint));
                }
                if (MakeSignedPing(currentThreadId, endPoint))
                {
                    succeededEndPoint = endPoint;
                    return true;
                }
            }
            succeededEndPoint = null;
            return false;
        }

        private void SendEnvironmentOperation(Guid recipientId, 
            EnvironmentOperationValue operationValue)
        {
            ComputingThreadWrapper thread;
            if (threads.TryGetValue(recipientId, out thread))
            {
                thread.Environment.NotifyValueReceived(operationValue);
            }
            else
            {
                // TODO: Handle the case of timeout smartly:
                // TODO: re-resolve, re-connect etc ...
                Send(ResolveThreadToEndPoint(operationValue.SenderId, recipientId), 
                    operationValue, Tags.EnvironmentOperation);
            }
        }

        #endregion

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
