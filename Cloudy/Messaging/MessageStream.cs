using System;
using System.IO;
using Cloudy.Protobuf;

namespace Cloudy.Messaging
{
    /// <summary>
    /// The utility class for streaming of messages.
    /// </summary>
    public class MessageStream
    {
        private readonly Stream inputStream;

        private readonly Stream outputStream;

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
        /// Reads a message from the input stream.
        /// </summary>
        /// <param name="type">The class of a message.</param>
        /// <returns>The read message or <c>null</c> at the end of the stream.</returns>
        public object Read(Type type)
        {
            lock (inputStream)
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
        /// Reads a message from the input stream.
        /// </summary>
        /// <typeparam name="T">The class of a message.</typeparam>
        /// <returns>The read message or <c>null</c> at the end of the stream.</returns>
        public T Read<T>()
        {
            return (T)Read(typeof(T));
        }

            /// <summary>
        /// Writes the message to the output stream.
        /// </summary>
        /// <typeparam name="T">The class of a message.</typeparam>
        public void Write<T>(T message)
        {
            lock (outputStream)
            {
                Serializer.CreateSerializer(typeof(T)).Serialize(
                    outputStream, true);
            }
        }

        /// <summary>
        /// Writes the message to the output stream.
        /// </summary>
        public void Write(object message)
        {
            lock (outputStream)
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
    }
}
