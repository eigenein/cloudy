using System;
using System.Net;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Messaging.Structures;
using Cloudy.Messaging.Enums;
using Cloudy.Messaging.Structures;

namespace Cloudy.Computing
{
    /// <summary>
    /// Represents an abstract slave node.
    /// </summary>
    public abstract class Slave : Node
    {
        private readonly IPEndPoint localEndPoint;

        protected Slave(IPEndPoint localEndPoint) 
            : base(localEndPoint.Port)
        {
            this.localEndPoint = localEndPoint;
        }

        // protected abstract 

        /// <summary>
        /// When overridden, creates a computing thread.
        /// </summary>
        protected abstract Action<IThreadingNetwork> CreateThread();

        /// <summary>
        /// Joins a network.
        /// </summary>
        /// <param name="remoteEndPoint">The master endpoint.</param>
        /// <param name="metadata">The metadata that will be associated with the slave.</param>
        public void JoinNetwork(IPEndPoint remoteEndPoint, byte[] metadata)
        {
            MessagingAsyncResult ar = Dispatcher.BeginSend(remoteEndPoint, 
                new JoinRequestValue(localEndPoint, metadata), 
                CommonTags.JoinRequest, null, null);
            Dispatcher.EndSend(ar, PingTimeout);
        }
    }
}
