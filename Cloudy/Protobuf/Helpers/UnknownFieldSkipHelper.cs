using System;
using System.Collections.Generic;
using System.IO;
using Cloudy.Protobuf.Encoding;
using Cloudy.Protobuf.Enums;

namespace Cloudy.Protobuf.Helpers
{
    /// <summary>
    /// The helper class - used to skip fields with unexpected field numbers.
    /// </summary>
    internal static class UnknownFieldSkipHelper
    {
        private static readonly Dictionary<WireType, Action<Stream>> Cache =
            new Dictionary<WireType, Action<Stream>>()
            {
                {WireType.Varint, SkipVarint},
                {WireType.LengthDelimited, SkipLengthDelimited},
                {WireType.Fixed32, SkipFixed32},
                {WireType.Fixed64, SkipFixed64}
            };

        /// <summary>
        /// Skips the field with the specified wire type.
        /// </summary>
        public static void Skip(Stream stream, WireType wireType)
        {
            Cache[wireType](stream);
        }

        private static void SkipVarint(Stream stream)
        {
            ProtobufReader.ReadUnsignedVarint(stream);
        }

        private static void SkipFixed32(Stream stream)
        {
            ProtobufReader.ReadRawBytes(stream, 4);
        }

        private static void SkipFixed64(Stream stream)
        {
            ProtobufReader.ReadRawBytes(stream, 8);
        }

        private static void SkipLengthDelimited(Stream stream)
        {
            ProtobufReader.ReadBytes(stream);
        }
    }
}
