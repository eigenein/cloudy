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
        private readonly Stream inputStream;

        private readonly object inputStreamLocker = new object();

        private readonly Stream outputStream;

        private readonly object outputStreamLocker = new object();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="stream">The underlying I/O stream.</param>
        public MessageStream(Stream stream)
            : this(stream, stream)
        {
            // Do nothing.
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="inputStream">The underlying input stream.</param>
        /// <param name="outputStream">The underlying output stream.</param>
        public MessageStream(Stream inputStream, Stream outputStream)
        {
            if (!inputStream.CanRead)
            {
                throw new ArgumentException("The inputStream is not readable.");
            }
            this.inputStream = inputStream;
            if (!outputStream.CanWrite)
            {
                throw new ArgumentException("The outputStream is not writable.");
            }
            this.outputStream = outputStream;
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
                        inputStream, true);
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
                    outputStream, true);
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
                    outputStream, message, true);
            }
        }

        /// <summary>
        /// Causes any buffered data to be written to the underlying device.
        /// </summary>
        public void Flush()
        {
            outputStream.Flush();
        }

        /// <summary>
        /// Closes the stream.
        /// </summary>
        public void Close()
        {
            inputStream.Close();
            if (!ReferenceEquals(inputStream, outputStream))
            {
                outputStream.Close();
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
                Close();
            }
        }
    }
}
