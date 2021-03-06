﻿using System;
using Cloudy.Messaging.Interfaces;
using Cloudy.Messaging.Structures;
using Cloudy.Protobuf;

namespace Cloudy.Messaging
{
    /// <summary>
    /// The utility class for streaming of messages.
    /// </summary>
    public class Communicator : IDisposable
    {
        private readonly ISimpleCommunicator simpleCommunicator;

        protected readonly object ReceiveLock = new object();
        protected readonly object SendLock = new object();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="simpleCommunicator">The underlying communicator.</param>
        public Communicator(ISimpleCommunicator simpleCommunicator)
        {
            this.simpleCommunicator = simpleCommunicator;
        }

        /// <summary>
        /// Gets the underlying communicator.
        /// </summary>
        public ISimpleCommunicator SimpleCommunicator
        {
            get { return simpleCommunicator; }
        }

        /// <summary>
        /// Reads a message from the communicator. The method is thread-safe.
        /// </summary>
        /// <param name="type">The class of a message.</param>
        /// <returns>The read message.</returns>
        public object Receive(Type type)
        {
            lock (ReceiveLock)
            {
                return Serializer.CreateSerializer(type).Deserialize(
                    simpleCommunicator.Receive());
            }
        }

        /// <summary>
        /// Reads a message from the communicator. The method is thread-safe.
        /// </summary>
        /// <typeparam name="T">The class of a message.</typeparam>
        /// <returns>The read message.</returns>
        public T Receive<T>()
        {
            return (T)Receive(typeof(T));
        }

        /// <summary>
        /// Writes the message to the communicator. The method is thread-safe.
        /// </summary>
        /// <typeparam name="T">The class of a message.</typeparam>
        public void Send<T>(T message)
        {
            lock (SendLock)
            {
                byte[] dgram = Serializer.CreateSerializer(
                    typeof(T)).Serialize(message);
                simpleCommunicator.Send(dgram);
            }
        }

        /// <summary>
        /// Writes the message to the communicator. The method is thread-safe.
        /// </summary>
        public void Send(object message)
        {
            lock (SendLock)
            {
                byte[] dgram = Serializer.CreateSerializer(
                    message.GetType()).Serialize(message);
                simpleCommunicator.Send(dgram);
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

        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
                simpleCommunicator.Dispose();
            }
        }
    }

    /// <summary>
    /// The utility class for streaming of messages.
    /// </summary>
    /// <typeparam name="TEndPoint">The class of endpoint.</typeparam>
    public class Communicator<TEndPoint> : Communicator
    {
        private readonly ISimpleCommunicator<TEndPoint> simpleCommunicator;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="simpleCommunicator">The underlying communicator.</param>
        public Communicator(ISimpleCommunicator<TEndPoint> simpleCommunicator) 
            : base(simpleCommunicator)
        {
            this.simpleCommunicator = simpleCommunicator;
        }

        /// <summary>
        /// Gets the underlying communicator.
        /// </summary>
        public new ISimpleCommunicator<TEndPoint> SimpleCommunicator
        {
            get { return simpleCommunicator; }
        }

        /// <summary>
        /// Reads a message from the communicator. The method is thread-safe.
        /// </summary>
        /// <param name="type">The class of a message.</param>
        /// <param name="endPoint">
        /// An endpoint that represents the 
        /// remote host from which the data was sent.
        /// </param>
        /// <returns>The read message.</returns>
        public object Receive(Type type, out TEndPoint endPoint)
        {
            lock (ReceiveLock)
            {
                return Serializer.CreateSerializer(type).Deserialize(
                    simpleCommunicator.Receive(out endPoint));
            }
        }

        /// <summary>
        /// Reads a message from the communicator. The method is thread-safe.
        /// </summary>
        /// <param name="endPoint">
        /// An endpoint that represents the 
        /// remote host from which the data was sent.
        /// </param>
        /// <typeparam name="T">The class of a message.</typeparam>
        /// <returns>The read message.</returns>
        public T Receive<T>(out TEndPoint endPoint)
        {
            return (T)Receive(typeof(T), out endPoint);
        }

        /// <summary>
        /// Writes the message to the communicator. The method is thread-safe.
        /// </summary>
        /// <typeparam name="T">The class of a message.</typeparam>
        public void Send<T>(T message, TEndPoint endPoint)
        {
            lock (SendLock)
            {
                byte[] dgram = Serializer.CreateSerializer(
                    typeof(T)).Serialize(message);
                simpleCommunicator.Send(dgram, endPoint);
            }
        }

        /// <summary>
        /// Writes the message to the communicator. The method is thread-safe.
        /// </summary>
        public void Send(object message, TEndPoint endPoint)
        {
            lock (SendLock)
            {
                byte[] dgram = Serializer.CreateSerializer(
                    message.GetType()).Serialize(message);
                simpleCommunicator.Send(dgram, endPoint);
            }
        }
    }
}
