using System;
using System.IO;
using Cloudy.Messaging.Interfaces;
using Cloudy.Protobuf.Encoding;

namespace Cloudy.Helpers
{
    /// <summary>
    /// A helper class that adapts any stream
    /// to the <see cref="ISenderReceiver"/> interface.
    /// </summary>
    public class StreamSenderReceiver : ISenderReceiver
    {
        private readonly Stream stream;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="stream">The underlying stream.</param>
        public StreamSenderReceiver(Stream stream)
        {
            this.stream = stream;
        }

        #region Implementation of ISenderReceiver

        /// <summary>
        /// Sends a byte array.
        /// </summary>
        public void Send(byte[] data)
        {
            if (!stream.CanWrite)
            {
                throw new InvalidOperationException("The stream should be writable.");
            }
            ProtobufWriter.WriteBytes(stream, data);
        }

        /// <summary>
        /// Receives a data.
        /// </summary>
        /// <returns>Received data. Indicates the end of stream if empty array is returned.</returns>
        public byte[] Receive()
        {
            if (!stream.CanRead)
            {
                throw new InvalidOperationException("The stream should be readable.");
            }
            return ProtobufReader.ReadBytes(stream);
        }

        /// <summary>
        /// Closes the sender-receiver.
        /// </summary>
        public void Close()
        {
            stream.Close();
        }

        #endregion
    }
}
