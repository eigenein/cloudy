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
        private readonly IPEndPoint sourceEndPoint;

        private readonly IPEndPoint externalEndPoint;

        protected Slave(IPEndPoint sourceEndPoint, IPEndPoint externalEndPoint) 
            : base(sourceEndPoint.Port)
        {
            this.sourceEndPoint = sourceEndPoint;
            this.externalEndPoint = externalEndPoint;
        }

        protected abstract TimeSpan SendTimeout { get; }

        // protected abstract 

        /// <summary>
        /// When overridden, creates a computing thread.
        /// </summary>
        protected abstract Action<INetwork> CreateThread();

        /// <summary>
        /// Joins a network.
        /// </summary>
        /// <param name="remoteEndPoint">The master endpoint.</param>
        public void JoinNetwork(IPEndPoint remoteEndPoint)
        {
            MessagingAsyncResult ar;
            ar = Dispatcher.BeginSend(remoteEndPoint, new JoinRequestValue(
                sourceEndPoint, externalEndPoint), CommonTags.JoinRequest, null, null);
            Dispatcher.EndSend(ar, SendTimeout);
            
        }
    }
}
