using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Cloudy.Computing.Enums;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Structures.Values;
using Cloudy.Computing.Topologies;
using Cloudy.Computing.Topologies.Interfaces;
using Cloudy.Computing.Topologies.Interfaces.Slave;
using Cloudy.Computing.Topologies.Structures;
using Cloudy.Helpers;
using Cloudy.Messaging.Interfaces;
using Cloudy.Structures;

namespace Cloudy.Computing
{
    public abstract class AbstractSlaveNode<TRank> : AbstractNode, IEnvironmentTransport
        where TRank : IRank
    {
        private readonly IPAddress localAddress;

        private IPEndPoint masterEndPoint;

        private SlaveState state;

        private ITopology topology;

        /// <summary>
        /// Use this lock every time you need a bi-directional conversation
        /// with the Master using both Send and Receive methods. One-directional
        /// replies and notifications don't need this lock.
        /// </summary>
        private readonly object masterConversationLock = new object();

        /// <summary>
        /// Holds local threads.
        /// </summary>
        private readonly Dictionary<byte[], ComputingThreadWrapper> threads =
            new Dictionary<byte[], ComputingThreadWrapper>(ByteArrayComparer.Instance);

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
            SignedPingResponseTimeout = new TimeSpan(0, 0, 7);
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

        public TimeSpan SignedPingResponseTimeout { get; set; }

        public event ParameterizedEventHandler<IPEndPoint, Guid> Joined;

        public event ParameterizedEventHandler<SlaveState> StateChanged;

        public event ParameterizedEventHandler<byte[]> ThreadStarted;

        public event ParameterizedEventHandler<byte[]> ThreadStopped;

        public event ParameterizedEventHandler<Exception> ExceptionUnhandled;

        public event ParameterizedEventHandler<byte[], IPEndPoint> CreatingWormHole;

        public event ParameterizedEventHandler<IPEndPoint, IPEndPoint> PortScanning;

        public event ParameterizedEventHandler<IPEndPoint, IPEndPoint> SignedPingRequested;

        public event ParameterizedEventHandler<ICollection<byte[]>, bool> SignedPingFinished;

        public event ParameterizedEventHandler<byte[], IPEndPoint> EndPointResolved;

        /// <summary>
        /// Creates a thread within this slave node.
        /// </summary>
        protected abstract IComputingThread CreateThread();

        public void Join(IPEndPoint endPoint)
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
            topology = SlaveTopologiesCache.GetTopology(response.TopologyType);
            if (topology.RankType != typeof(TRank))
            {
                throw new InvalidOperationException(String.Format(
                    "Rank type mismatch: {0} and {1}", topology.RankType, typeof(TRank)));
            }
            State = SlaveState.Joined;
            if (Joined != null)
            {
                Joined(this, new EventArgs<IPEndPoint, Guid>(
                    response.ExternalEndPoint.Value, response.SlaveId));
            }
            return;
        }

        private void OnStartThread(IPEndPoint remoteEndPoint, IMessage message)
        {
            byte[] rank = message.Get<WrappedValue<byte[]>>().Value;
            ComputingThreadWrapper thread;
            if (!threads.TryGetValue(rank, out thread))
            {
                thread = threads[rank] = new ComputingThreadWrapper(
                    rank, new Environment<TRank>(this, rank), CreateThread);
                thread.ThreadCompleted += OnThreadCompleted;
                thread.ThreadFailed += OnThreadFailed;
                thread.ThreadStopped += OnThreadStopped;
            }
            thread.Restart();
            if (ThreadStarted != null)
            {
                ThreadStarted(this, new EventArgs<byte[]>(rank));
            }
        }

        private void OnThreadCompleted(object sender, EventArgs<byte[]> e)
        {
            Send(masterEndPoint, new WrappedValue<byte[]> { Value = e.Value },
                Tags.ThreadCompleted);
        }

        /// <summary>
        /// Called when the thread is abnormally terminated.
        /// </summary>
        /// <param name="e">Contains an exception object if any have been thrown.</param>
        private void OnThreadFailed(object sender, EventArgs<byte[], Exception> e)
        {
            Send(masterEndPoint, new WrappedValue<byte[]> { Value = e.Value1 }, 
                Tags.ThreadFailed);
            if (e.Value2 != null && ExceptionUnhandled != null)
            {
                ExceptionUnhandled(this, new EventArgs<Exception>(e.Value2));
            }
        }

        private void OnStopThread(IPEndPoint remoteEndPoint, IMessage message)
        {
            byte[] rank = message.Get<WrappedValue<byte[]>>().Value;
            ComputingThreadWrapper thread;
            if (threads.TryGetValue(rank, out thread))
            {
                thread.Abort();
            }
        }

        private void OnThreadStopped(object sender, EventArgs<byte[]> e)
        {
            OnThreadStopped(e.Value);
        }

        private void OnThreadStopped(byte[] rank)
        {
            if (ThreadStopped != null)
            {
                ThreadStopped(this, new EventArgs<byte[]>(rank));
            }
        }

        private void OnEnvironmentOperation(IPEndPoint remoteEndPoint, IMessage message)
        {
            EnvironmentOperationValue value = message.Get<EnvironmentOperationValue>();
            ICollection<byte[]> nextRecipientsIds = new HashSet<byte[]>();
            foreach (byte[] rank in value.Recipients)
            {
                ComputingThreadWrapper thread;
                if (threads.TryGetValue(rank, out thread))
                {
                    thread.Environment.NotifyValueReceived(value);
                }
                else
                {
                    nextRecipientsIds.Add(rank);
                }
            }
            value.Recipients = nextRecipientsIds;
            SendEnvironmentOperation(value); // Send further to the recipients left.
        }

        private void OnEndPointResolved(byte[] rank, IPEndPoint endPoint)
        {
            if (EndPointResolved != null)
            {
                EndPointResolved(this, new EventArgs<byte[], IPEndPoint>(
                    rank, endPoint));
            }
        }

        private void OnSignedPing(IPEndPoint remoteEndPoint, IMessage message)
        {
            byte[] rank = message.Get<WrappedValue<byte[]>>().Value;
            endPoints[rank] = remoteEndPoint;
            OnEndPointResolved(rank, remoteEndPoint);
        }

        private void OnSignedPingRequest(IPEndPoint remoteEndPoint, IMessage message)
        {
            SignedPingRequest request = message.Get<SignedPingRequest>();
            ThreadPool.UnsafeQueueUserWorkItem(
                o => ServeSignedPingRequest(remoteEndPoint, request), null);
        }

        private void ServeSignedPingRequest(IPEndPoint remoteEndPoint, SignedPingRequest request)
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
            IPEndPoint endPointSucceeded = 
                MakeSignedPing(senderLocalEndPoint) ? senderLocalEndPoint :
                (MakeSignedPing(senderExternalEndPoint) ? senderExternalEndPoint : null);
            if (endPointSucceeded != null)
            {
                response.Success = true;
                foreach (byte[] rank in request.LocalRanks)
                {
                    endPoints[rank] = endPointSucceeded;
                }
            }
            if (SignedPingFinished != null)
            {
                SignedPingFinished(this, new EventArgs<ICollection<byte[]>, bool>(
                    request.LocalRanks, response.Success == true));
            }
            Send(remoteEndPoint, response, Tags.SignedPingResponse);
        }

        private bool MakeSignedPing(IPEndPoint targetEndPoint)
        {
            try
            {
                SignedPingValue value = new SignedPingValue();
                value.LocalRanks = threads.Keys.ToList();
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

        #endregion

        #region Environment Operations

        private readonly object endPointsSynchronizationRoot = new object();

        /// <summary>
        /// Caches endpoints of the threads.
        /// </summary>
        private readonly Dictionary<byte[], IPEndPoint> endPoints =
            new Dictionary<byte[], IPEndPoint>(ByteArrayComparer.Instance);

        private void SendEnvironmentOperation(EnvironmentOperationValue operationValue)
        {
            if (operationValue.Recipients.Count == 0)
            {
                return;
            }
            Dictionary<byte[], List<byte[]>> nextRecipients = 
                new Dictionary<byte[], List<byte[]>>();
            foreach (byte[] recipient in operationValue.Recipients)
            {
                RouteSearchResult result = threads.Keys
                    .Select(rank => topology.TryFindNext(rank, recipient))
                    .Where(r => r.Success)
                    .OrderBy(r => r.Distance)
                    .FirstOrDefault();
                if (result == null)
                {
                    throw new InvalidOperationException(
                        "Could not find any route path.");
                }
                List<byte[]> list;
                if (!nextRecipients.TryGetValue(result.NextRank, out list))
                {
                    list = nextRecipients[result.NextRank] = new List<byte[]>();
                }
                list.Add(recipient);
            }
            foreach (KeyValuePair<byte[], List<byte[]>> recipient in nextRecipients)
            {
                operationValue.Recipients = recipient.Value;
                SendEnvironmentOperation(recipient.Key, operationValue);
            }
        }

        private IPEndPoint ResolveThreadToEndPoint(byte[] destination)
        {
            if (destination.Length == 0)
            {
                // The loopback ID.
                return LocalEndPoint;
            }
            lock (endPointsSynchronizationRoot)
            {
                IPEndPoint targetEndPoint;
                if (endPoints.TryGetValue(destination, out targetEndPoint))
                {
                    return targetEndPoint;
                }
                EndPointResponseValue response;
                lock (masterConversationLock)
                {
                    Send(masterEndPoint, new WrappedValue<byte[]> { Value = destination },
                        Tags.EndPointRequest);
                    response = ReceiveFrom<EndPointResponseValue>(masterEndPoint);
                }
                IPEndPoint succeededEndPoint;
                if (CreateWormhole(destination, response.LocalEndPoint.Value,
                    out succeededEndPoint) ||
                    CreateWormhole(destination, response.ExternalEndPoint.Value,
                    out succeededEndPoint))
                {
                    if (succeededEndPoint != null)
                    {
                        OnEndPointResolved(destination, succeededEndPoint);
                        return endPoints[destination] = succeededEndPoint;
                    }
                    return endPoints[destination];
                }
                // TODO: Handle this case more smartly:
                // TODO: re-resolve, create a bridge through master etc ...
                throw new IOException(String.Format(
                    "Cannot create a wormhole to the thread {0} at {1}",
                    destination, response));
            }
        }

        /// <summary>
        /// Performs the UDP hole punching.
        /// </summary>
        /// <returns>Whether the method call was succedded.</returns>
        private bool CreateWormhole(byte[] destination,
            IPEndPoint targetEndPoint, out IPEndPoint succeededEndPoint)
        {
            succeededEndPoint = targetEndPoint;
            if (CreatingWormHole != null)
            {
                CreatingWormHole(this, new EventArgs<byte[], IPEndPoint>(
                    destination, targetEndPoint));
            }
            if (MakeSignedPing(targetEndPoint))
            {
                // Hooray! We've established a connection!
                return true;
            }
            // No, then we should make request for the external ping.
            lock (masterConversationLock)
            {
                SignedPingRequest request = new SignedPingRequest();
                request.LocalRanks = threads.Keys.ToList();
                request.Destination = destination;
                request.SenderLocalEndPoint.Value = LocalEndPoint;
                request.SenderExternalEndPoint.Value = ExternalEndPoint;
                Send(masterEndPoint, request, Tags.SignedPingRequest);
                SignedPingResponse response = ReceiveFrom<SignedPingResponse>(
                    masterEndPoint, SignedPingResponseTimeout);
                if (response.Success == true)
                {
                    /* 
                     * This means that we've already cached the endpoint when
                     * handling an incoming SignedPing. Also, the method
                     * doesn't know the actual endpoint, thus it will leave
                     * the succeededEndPoint null.
                     */
                    succeededEndPoint = null;
                    return true;
                }
                return DoPortScan(targetEndPoint, out succeededEndPoint);
            }
        }

        /// <summary>
        /// Attempts to find the external port number of the node.
        /// </summary>
        /// <param name="initialEndPoint">The endpoint to start with.</param>
        /// <param name="succeededEndPoint"></param>
        /// <returns>Whether a call was successful.</returns>
        private bool DoPortScan(IPEndPoint initialEndPoint,
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
                if (MakeSignedPing(endPoint))
                {
                    succeededEndPoint = endPoint;
                    return true;
                }
            }
            succeededEndPoint = null;
            return false;
        }

        private void SendEnvironmentOperation(byte[] recipient, 
            EnvironmentOperationValue operationValue)
        {
            ComputingThreadWrapper thread;
            if (threads.TryGetValue(recipient, out thread))
            {
                thread.Environment.NotifyValueReceived(operationValue);
            }
            else
            {
                // TODO: Handle the case of timeout smartly:
                // TODO: re-resolve, re-connect etc ...
                Send(ResolveThreadToEndPoint(recipient), operationValue, 
                    Tags.EnvironmentOperation);
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
