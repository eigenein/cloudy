using System;
using System.IO;

namespace Cloudy.Protobuf.Helpers
{
    public class StreamSegment : Stream
    {
        private readonly Stream stream;

        private long length;

        private readonly long position;

        public StreamSegment(Stream stream, long length)
        {
            if (!stream.CanSeek)
            {
                throw new NotSupportedException("The underlying stream must be seekable.");
            }
            this.stream = stream;
            this.position = stream.Position;
            this.length = length;
        }

        #region Overrides of Stream

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            this.length = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return stream.Read(buffer, offset, 
                Math.Min(count, (int)(stream.Position - position)));
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
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
            get { return false; }
        }

        public override long Length
        {
            get { return length; }
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        #endregion
    }
}
