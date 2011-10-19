using System;
using System.IO;
using Cloudy.Protobuf.Enums;

namespace Cloudy.Protobuf.Encoding
{
    public static class ProtobufWriter
    {
        public static void WriteRawBytes(Stream stream, byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteBytes(Stream stream, byte[] bytes)
        {
            WriteUnsignedVarint(stream, (ulong)bytes.Length);
            WriteRawBytes(stream, bytes);
        }

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

        public static void WriteSignedVarint(Stream stream, long value)
        {
            WriteUnsignedVarint(stream, (ulong)(value << 1 ^ value >> 63));
        }

        public static void WriteSignedVarint(Stream stream, int value)
        {
            WriteUnsignedVarint(stream, (ulong)(value << 1 ^ value >> 31));
        }

        public static void WriteUnsignedVarint(Stream stream, uint value)
        {
            WriteUnsignedVarint(stream, (ulong)value);
        }

        public static void WriteKey(Stream stream, uint fieldNumber, WireType wireType)
        {
            WriteUnsignedVarint(stream, ((fieldNumber) << 3) | (uint)wireType);
        }
    }
}
