using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Cloudy.Helpers
{
    /// <summary>
    /// Wraps a <see cref="System.Net.Sockets.UdpClient"/> to provide a standard
    /// <see cref="System.IO.Stream"/> interface to the wrapped UDP client.
    /// </summary>
    public class UdpStream : Stream
    {
        #region Private Fields

        private readonly UdpClient client;

        private byte[] bufferedBytes;

        private int bufferedBytesOffset;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="client">The underlying UDP client.</param>
        public UdpStream(UdpClient client)
        {
            this.client = client;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the underlying UDP client.
        /// </summary>
        public UdpClient Client
        {
            get { return client; }
        }

        #endregion

        #region Overrides of Stream

        public override void Flush()
        {
            // Do nothing - no need to flush.
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(
                "Unable to set the position of the UDP stream.");
        }

        public override void SetLength(long value)
        {
           throw new NotSupportedException(
                "Unable to set the length of the UDP stream.");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;
            while (bytesRead < count)
            {
                int bufferedBytesCount = bufferedBytes != null ?
                    bufferedBytes.Length - bufferedBytesOffset : 0;
                int partLength;
                if (bufferedBytesCount > 0)
                {
                    // If there are buffered bytes then read from the buffer.
                    partLength = Math.Min(count, bufferedBytesCount);
                }
                else
                {
                    // Fill in the buffer with the data read.
                    IPEndPoint endPoint = null;
                    byte[] bytesReceived = client.Receive(ref endPoint);
                    // Rewind the buffer.
                    bufferedBytes = bytesReceived;
                    bufferedBytesOffset = 0;
                    partLength = Math.Min(count, bytesReceived.Length);
                }
                Array.Copy(bufferedBytes, bufferedBytesOffset, buffer, offset, partLength);
                // Move the source buffer pointer.
                bufferedBytesOffset += partLength;
                // Move the destination buffer pointer.
                count -= partLength;
                offset += partLength;
                bytesRead += partLength;
            }
            return bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (offset == 0)
            {
                // Optimize the common case.
                client.Send(buffer, count);
            }
            else
            {
                byte[] dgram = new byte[count];
                Array.Copy(buffer, offset, dgram, 0, count);
                client.Send(dgram, count);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                client.Close();
            }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { throw new NotSupportedException(
                "Unable to determine the length of the UDP stream."); }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException(
                    "Unable to determine the position of the UDP stream.");
            }
            set
            {
                throw new NotSupportedException(
                    "Unable to set the position of the UDP stream.");
            }
        }

        #endregion
    }
}
