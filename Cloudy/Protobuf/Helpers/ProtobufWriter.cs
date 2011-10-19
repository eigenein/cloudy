using System;
using System.IO;
using System.Text;
using Cloudy.Protobuf.Enums;

namespace Cloudy.Protobuf.Helpers
{
    /// <summary>
    /// An utility class for writing Protocol Buffers values.
    /// </summary>
    public static class ProtobufWriter
    {
        public static void WriteKey(int tag, WireType wireType, Stream stream)
        {
            WriteVarint((tag << 3) | (int)wireType, stream);
        }

        public static void WriteUnsignedVarint(ulong value, Stream stream)
        {
            if (value <= 0x80)
            {
                stream.WriteByte((byte)value);
            }
            else
            {
                do
                {
                    byte bits = (byte)(value & 0x7f);
                    value >>= 7;
                    if (value != 0)
                    {
                        bits |= 0x80;
                    }
                    stream.WriteByte(bits);
                } while (value != 0);
            }
        }

        public static void WriteUnsignedVarint(uint value, Stream stream)
        {
            WriteUnsignedVarint((ulong)value, stream);
        }

        public static void WriteVarint(long value, Stream stream)
        {
            WriteUnsignedVarint((ulong)(value << 1 ^ value >> 63), stream);
        }

        public static void WriteVarint(int value, Stream stream)
        {
            WriteUnsignedVarint((ulong)(value << 1 ^ value >> 31), stream);
        }

        public static void WriteBytes(byte[] value, Stream stream)
        {
            WriteUnsignedVarint((ulong)value.Length, stream);
            stream.Write(value, 0, value.Length);
        }

        public static void WriteString(string value, Stream stream)
        {
            WriteBytes(Encoding.UTF8.GetBytes(value), stream);
        }
    }
}
