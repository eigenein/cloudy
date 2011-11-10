using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Threading;
using Cloudy.Helpers;
using Cloudy.Messaging;
using Cloudy.Messaging.Interfaces;
using Cloudy.Networking.Dto;
using Cloudy.Protobuf;

namespace Cloudy.Networking.IP
{
    /// <summary>
    /// Represents a service that tells to a client its (client's) external
    /// IP address and port.
    /// </summary>
    public class IPEndPointServer
    {
        private Thread processingThread;

        private readonly UdpClient client;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="portNumber">The port to listen to.</param>
        public IPEndPointServer(int portNumber)
        {
            this.client = new UdpClient(portNumber);
        }

        public event EventHandler InvalidRequestReceived;

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
        private void ProcessOneRequest()
        {
            IPEndPoint remoteEndPoint = null;
            byte[] request = client.Receive(ref remoteEndPoint);
            // Check if this is a magic datagram.
            if (request.Length != 1 || request[0] != 0xAA)
            {
                OnInvalidRequestReceived();
                return;
            }
            byte[] dgram = Serializer.CreateSerializer(typeof(IPEndPointDto)).Serialize(
                new IPEndPointDto(remoteEndPoint));
            client.Send(dgram, dgram.Length, remoteEndPoint);
        }

        private void OnInvalidRequestReceived()
        {
            EventHandler handler = InvalidRequestReceived;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        /// <summary>
        /// Runs an infinite loop of processing requests.
        /// </summary>
        private void ProcessRequests()
        {
            while (processingThread.ThreadState != ThreadState.AbortRequested)
            {
                try
                {
                    ProcessOneRequest();
                }
                catch (ThreadAbortException)
                {
                    return;
                }
            }
        }

        private void Close()
        {
            client.Close();
        }
    }
}
