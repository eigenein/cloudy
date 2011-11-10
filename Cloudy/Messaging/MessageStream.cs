using System;
using System.Net;
using System.Net.Sockets;
using Cloudy.Messaging.Interfaces;
using Cloudy.Messaging.Structures;
using Cloudy.Protobuf;

namespace Cloudy.Messaging
{
    /// <summary>
    /// The utility class for streaming of messages.
    /// </summary>
    public class MessageStream : IDisposable
    {
        private readonly ISenderReceiver senderReceiver;

        private readonly object inputStreamLocker = new object();

        private readonly object outputStreamLocker = new object();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="senderReceiver">The underlying sender-receiver.</param>
        public MessageStream(ISenderReceiver senderReceiver)
        {
            this.senderReceiver = senderReceiver;
        }

        /// <summary>
        /// Gets the underlying sender-receiver.
        /// </summary>
        public ISenderReceiver SenderReceiver
        {
            get { return senderReceiver; }
        }

        /// <summary>
        /// Reads a message from the input stream. The method is thread-safe.
        /// </summary>
        /// <param name="type">The class of a message.</param>
        /// <returns>The read message.</returns>
        public object Read(Type type)
        {
            lock (inputStreamLocker)
            {
                return Serializer.CreateSerializer(type).Deserialize(
                    senderReceiver.Receive());
            }
        }

        /// <summary>
        /// Reads a message from the input stream. The method is thread-safe.
        /// </summary>
        /// <typeparam name="T">The class of a message.</typeparam>
        /// <returns>The read message or <c>null</c> at the end of the stream.</returns>
        public T Read<T>()
        {
            return (T)Read(typeof(T));
        }

        /// <summary>
        /// Reads a tagged message from the input stream. The method is thread-safe.
        /// </summary>
        /// <returns>The read message or <c>null</c> at the end of the stream.</returns>
        public ICastableValue Read(out int? tag)
        {
            Dto dto = Read<Dto>();
            tag = dto.Tag;
            return dto;
        }

        /// <summary>
        /// Writes the message to the output stream. The method is thread-safe.
        /// </summary>
        /// <typeparam name="T">The class of a message.</typeparam>
        public void Write<T>(T message)
        {
            lock (outputStreamLocker)
            {
                byte[] dgram = Serializer.CreateSerializer(
                    typeof(T)).Serialize(message);
                senderReceiver.Send(dgram);
            }
        }

        /// <summary>
        /// Writes the message to the output stream. The method is thread-safe.
        /// </summary>
        public void Write(object message)
        {
            lock (outputStreamLocker)
            {
                byte[] dgram = Serializer.CreateSerializer(
                    message.GetType()).Serialize(message);
                senderReceiver.Send(dgram);
            }
        }

        /// <summary>
        /// Writes the tagged message to the output stream. The method is thread-safe.
        /// </summary>
        public void Write(int? tag, object message)
        {
            Write(new Dto(tag, Serializer.CreateSerializer(
                message.GetType()).Serialize(message)));
        }

        /// <summary>
        /// Writes the tagged message to the output stream. The method is thread-safe.
        /// </summary>
        public void Write<T>(int? tag, T message)
        {
            Write(new Dto<T>(tag, message));
        }

        /// <summary>
        /// Closes the stream.
        /// </summary>
        public void Close()
        {
            senderReceiver.Close();
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
                Close();
            }
        }
    }
}
