using System;
using System.IO;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Exceptions;

namespace Cloudy.Protobuf.Encoding
{
    /// <summary>
    /// The helper class - used to decode Protobuf encoding.
    /// </summary>
    public static class ProtobufReader
    {
        /// <summary>
        /// Reads the specified count of bytes from the stream "as is".
        /// </summary>
        public static byte[] ReadRawBytes(Stream stream, int count)
        {
            byte[] bytes = new byte[count];
            if (stream.Read(bytes, 0, count) < count)
            {
                throw new UnexpectedEndOfStreamException("Unexpected end of stream.");
            }
            return bytes;
        }

        /// <summary>
        /// Reads and decodes the byte array from the Protobuf input stream.
        /// </summary>
        public static byte[] ReadBytes(Stream stream)
        {
            ulong length = ReadUnsignedVarint(stream);
            return ReadRawBytes(stream, (int)length);
        }

        /// <summary>
        /// Reads and decodes an unsigned Varint from the Protobuf stream.
        /// </summary>
        public static ulong ReadUnsignedVarint(Stream stream)
        {
            ulong value = 0;
            int shift = 0;
            bool continuationExpected = false;
            while (true)
            {
                int quantum = stream.ReadByte();
                if (quantum == -1)
                {
                    throw continuationExpected ?
                        new UnexpectedEndOfStreamException(
                            "Unexpected end of stream while reading a Varint value.") :
                        (Exception)new EndOfStreamException();
                }
                value += ((ulong)quantum & 0x7Ful) << shift;
                if (!(continuationExpected = (quantum & 0x80) == 0x80))
                {
                    break;
                }
                shift += 7;
            }
            return value;
        }

        /// <summary>
        /// Reads and decodes a signed Varint from the Protobuf stream.
        /// </summary>
        public static long ReadSignedVarint(Stream stream)
        {
            ulong value = ReadUnsignedVarint(stream);
            ulong mask = 0L - (value & 1);
            return (long)(value >> 1 ^ mask);
        }

        /// <summary>
        /// Reads and decodes a key from the Protobuf stream.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="fieldNumber">Decoded field number.</param>
        /// <param name="wireType">Decoded wire type.</param>
        public static void ReadKey(Stream stream, out uint fieldNumber,
            out WireType wireType)
        {
            ulong value = ReadUnsignedVarint(stream);
            wireType = (WireType)(value & 0x7ul);
            fieldNumber = (uint)(value >> 3);
        }
    }
}
