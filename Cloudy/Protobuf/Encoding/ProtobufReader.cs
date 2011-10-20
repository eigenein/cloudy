using System;
using System.IO;
using Cloudy.Protobuf.Enums;

namespace Cloudy.Protobuf.Encoding
{
    public static class ProtobufReader
    {
        public static byte[] ReadRawBytes(Stream stream, int count)
        {
            byte[] bytes = new byte[count];
            if (stream.Read(bytes, 0, count) < count)
            {
                throw new InvalidDataException("Unexpected end of stream.");
            }
            return bytes;
        }

        public static byte[] ReadBytes(Stream stream)
        {
            ulong length = ReadUnsignedVarint(stream);
            return ReadRawBytes(stream, (int)length);
        }

        public static ulong ReadUnsignedVarint(Stream stream)
        {
            ulong value = 0;
            int shift = 0, quantum = 0x80;
            while ((quantum & 0x80) == 0x80)
            {
                quantum = stream.ReadByte();
                if (quantum == -1)
                {
                    throw new EndOfStreamException("End of stream while reading a Varint value.");
                }
                value += ((ulong)quantum & 0x7Ful) << shift;
                shift += 7;
            }
            return value;
        }

        public static long ReadSignedVarint(Stream stream)
        {
            ulong value = ReadUnsignedVarint(stream);
            ulong mask = 0L - (value & 1);
            return (long)(value >> 1 ^ mask);
        }

        public static void ReadKey(Stream stream, out uint fieldNumber,
            out WireType wireType)
        {
            ulong value = ReadUnsignedVarint(stream);
            wireType = (WireType)(value & 0x7ul);
            fieldNumber = (uint)(value >> 3);
        }
    }
}
