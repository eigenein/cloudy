using System;
using System.IO;
using Cloudy.Protobuf.Enums;

namespace Cloudy.Protobuf.Encoding
{
    /// <summary>
    /// The helper class - used to encode data into the Protobuf format.
    /// </summary>
    public static class ProtobufWriter
    {
        /// <summary>
        /// Writes the byte array to the output stream "as is".
        /// </summary>
        public static void WriteRawBytes(Stream stream, byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Encodes and writes the byte array into the output Protobuf stream.
        /// </summary>
        public static void WriteBytes(Stream stream, byte[] bytes)
        {
            WriteUnsignedVarint(stream, (ulong)bytes.Length);
            WriteRawBytes(stream, bytes);
        }

        /// <summary>
        /// Encodes and writes the unsigned Varint value.
        /// </summary>
        public static void WriteUnsignedVarint(Stream stream, ulong value)
        {
            if (value < 0x80)
            {
                stream.WriteByte((byte)value);
            }
            else
            {
                while (value != 0)
                {
                    ulong shiftedValue = value >> 7;
                    stream.WriteByte((byte)((uint)(value & 0x7Fu) |
                        (shiftedValue != 0 ? 0x80u : 0x00u)));
                    value = shiftedValue;
                }
            }
        }

        /// <summary>
        /// Encodes and writes the signed Varint value.
        /// </summary>
        public static void WriteSignedVarint(Stream stream, long value)
        {
            WriteUnsignedVarint(stream, (ulong)(value << 1 ^ value >> 63));
        }

        public static void WriteSignedVarint(Stream stream, int value)
        {
            WriteUnsignedVarint(stream, (ulong)(value << 1 ^ value >> 31));
        }

        /// <summary>
        /// Encodes and writes the unsigned Varint value.
        /// </summary>
        private static void WriteUnsignedVarint(Stream stream, uint value)
        {
            WriteUnsignedVarint(stream, (ulong)value);
        }

        /// <summary>
        /// Encodes and writes the key.
        /// </summary>
        public static void WriteKey(Stream stream, uint fieldNumber, WireType wireType)
        {
            WriteUnsignedVarint(stream, ((fieldNumber) << 3) | (uint)wireType);
        }
    }
}
