using System;
using System.IO;
using Cloudy.Messaging.Interfaces;
using Cloudy.Protobuf.Encoding;

namespace Cloudy.Helpers
{
    /// <summary>
    /// A helper class that adapts any stream
    /// to the <see cref="IRawCommunicator"/> interface.
    /// </summary>
    public class StreamRawCommunicator<TEndPoint> : IRawCommunicator<TEndPoint>
    {
        private readonly Stream stream;

        private readonly TEndPoint defaultEndPoint;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="stream">The underlying stream.</param>
        /// <param name="defaultEndPoint">The fake endpoint for the stream.</param>
        public StreamRawCommunicator(Stream stream, TEndPoint defaultEndPoint)
        {
            this.stream = stream;
            this.defaultEndPoint = defaultEndPoint;
        }

        #region Implementation of IRawCommunicator

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

        #region Implementation of IRawCommunicator<TEndPoint>

        /// <summary>
        /// Sends a byte array.
        /// </summary>
        public void Send(byte[] data, TEndPoint endPoint)
        {
            if (!defaultEndPoint.Equals(endPoint))
            {
                throw new InvalidOperationException("Cannot send to a different endpoint.");
            }
            Send(data);
        }

        /// <summary>
        /// Receives a data.
        /// </summary>
        /// <returns>Received data. Indicates the end of stream if empty array is returned.</returns>
        public byte[] Receive(out TEndPoint endPoint)
        {
            endPoint = defaultEndPoint;
            return Receive();
        }

        #endregion
    }
}
