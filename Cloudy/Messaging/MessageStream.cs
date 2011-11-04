using System;
using System.IO;
using Cloudy.Protobuf;

namespace Cloudy.Messaging
{
    /// <summary>
    /// The utility class for streaming of messages.
    /// </summary>
    public class MessageStream : IDisposable
    {
        private readonly Stream stream;

        private readonly object inputStreamLocker = new object();

        private readonly object outputStreamLocker = new object();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="stream">The underlying I/O stream.</param>
        public MessageStream(Stream stream)
        {
            this.stream = stream;
        }

        /// <summary>
        /// Gets the underlying stream.
        /// </summary>
        public Stream Stream
        {
            get { return stream; }
        }

        /// <summary>
        /// Indicates whether the current stream supports reading.
        /// </summary>
        public bool CanRead
        {
            get { return stream.CanRead; }
        }

        /// <summary>
        /// Indicates whether the current stream supports writing.
        /// </summary>
        public bool CanWrite
        {
            get { return stream.CanWrite; }
        }

        /// <summary>
        /// Reads a message from the input stream. The method is thread-safe.
        /// </summary>
        /// <param name="type">The class of a message.</param>
        /// <returns>The read message or <c>null</c> at the end of the stream.</returns>
        public object Read(Type type)
        {
            lock (inputStreamLocker)
            {
                try
                {
                    return Serializer.CreateSerializer(type).Deserialize(
                        stream, true);
                }
                catch (EndOfStreamException)
                {
                    return null;
                }
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
        /// Writes the message to the output stream. The method is thread-safe.
        /// </summary>
        /// <typeparam name="T">The class of a message.</typeparam>
        public void Write<T>(T message)
        {
            lock (outputStreamLocker)
            {
                Serializer.CreateSerializer(typeof(T)).Serialize(
                    stream, message, true);
            }
        }

        /// <summary>
        /// Writes the message to the output stream. The method is thread-safe.
        /// </summary>
        public void Write(object message)
        {
            lock (outputStreamLocker)
            {
                Serializer.CreateSerializer(message.GetType()).Serialize(
                    stream, message, true);
            }
        }

        /// <summary>
        /// Causes any buffered data to be written to the underlying device.
        /// </summary>
        public void Flush()
        {
            stream.Flush();
        }

        /// <summary>
        /// Closes the stream.
        /// </summary>
        public void Close()
        {
            stream.Close();
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
