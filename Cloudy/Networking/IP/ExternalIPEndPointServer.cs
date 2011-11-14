using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading;
using Cloudy.Helpers;
using Cloudy.Messaging;
using Cloudy.Messaging.Enums;
using Cloudy.Messaging.Interfaces;
using Cloudy.Networking.Events;
using Cloudy.Networking.Values;

namespace Cloudy.Networking.IP
{
    /// <summary>
    /// Represents a service that tells to a client its (client's) external
    /// IP address and port.
    /// </summary>
    public class ExternalIPEndPointServer : IDisposable
    {
        private Thread processingThread;

        private readonly Communicator<IPEndPoint> communicator;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="portNumber">The port to listen to.</param>
        public ExternalIPEndPointServer(int portNumber)
        {
            this.communicator = new Communicator<IPEndPoint>(
                new UdpClientRawCommunicator(new UdpClient(portNumber)));
        }

        /// <summary>
        /// Occurs when non-request or invalid request is received.
        /// </summary>
        public event EventHandler InvalidRequestReceived;

        /// <summary>
        /// Occurs when a correct request are received.
        /// </summary>
        public event EventHandler<ExternalIPEndPointRequestedEventArgs> ExternalIPEndPointRequested;

        /// <summary>
        /// Starts processing of requests.
        /// </summary>
        public void Start()
        {
            Thread thread = processingThread;
            if (thread != null)
            {
                throw new InvalidOperationException("Already started.");
            }
            processingThread = new Thread(ProcessRequests);
            processingThread.IsBackground = true;
            processingThread.Start();
        }

        /// <summary>
        /// Stops processing of requests.
        /// </summary>
        public void Stop()
        {
            Thread thread = processingThread;
            if (thread == null)
            {
                throw new InvalidOperationException("Already stopped.");
            }
            processingThread = null;
            thread.Abort();
        }

        /// <summary>
        /// Processes a single incoming request.
        /// </summary>
        /// <returns>Whether the request was correct.</returns>
        private bool ProcessOneRequest()
        {
            int? tag;
            IPEndPoint remoteEndPoint;
            ICastable message = communicator.ReceiveTagged(out tag, out remoteEndPoint);
            if (tag != WellKnownTags.ExternalIPEndPointRequest)
            {
                return false;
            }
            ExternalIPEndPointRequest request = message.Cast<ExternalIPEndPointRequest>();
            OnExternalIPEndPointRequested(remoteEndPoint);
            communicator.SendTagged(WellKnownTags.ExternalIPEndPointResponse,
                new ExternalIPEndPointResponse(request.Id, remoteEndPoint), remoteEndPoint);
            return true;
        }

        private void OnInvalidRequestReceived()
        {
            EventHandler handler = InvalidRequestReceived;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        private void OnExternalIPEndPointRequested(IPEndPoint endPoint)
        {
            EventHandler<ExternalIPEndPointRequestedEventArgs> handler =
                ExternalIPEndPointRequested;
            if (handler != null)
            {
                handler(this, new ExternalIPEndPointRequestedEventArgs(endPoint));
            }
        }

        /// <summary>
        /// Runs an infinite loop of processing requests.
        /// </summary>
        private void ProcessRequests()
        {
            try
            {
                while (processingThread.ThreadState != ThreadState.AbortRequested)
                {
                    bool result = false;
                    try
                    {
                        result = ProcessOneRequest();
                    }
                    catch (SerializationException)
                    {
                        continue;
                    }
                    finally
                    {
                        if (!result)
                        {
                            OnInvalidRequestReceived();
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                communicator.Dispose();
            }
        }
    }
}
