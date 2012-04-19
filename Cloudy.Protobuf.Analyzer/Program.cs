using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Cloudy.Protobuf.Encoding;
using Cloudy.Protobuf.Enums;

using NLog;

namespace Cloudy.Protobuf.Analyzer
{
    public static class Program
    {
        private static readonly Logger Logger =
            LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<WireType, Action<Stream, int>> Analyzers;

        static Program()
        {
            Analyzers = new Dictionary<WireType, Action<Stream, int>>();
            Analyzers.Add(WireType.LengthDelimited, AnalyzeLengthDelimited);
            Analyzers.Add(WireType.Varint, AnalyzeVarint);
            Analyzers.Add(WireType.Fixed32, AnalyzeFixed32);
            Analyzers.Add(WireType.Fixed64, AnalyzeFixed64);
        }

        #region Main

        public static void Main(string[] args)
        {
            Logger.Info(String.Empty);
            Logger.Info("Cloudy Protocol Buffers Analyzer");
            Logger.Info("Usage:");
            Logger.Info("    Cloudy.Protobuf.Analyzer.exe [Byte Array]");
            Logger.Info("If Byte Array is missing then it's read from stdin.");
            Logger.Info(String.Empty);
            Analyze(args.Length != 0 ? args[0] : Console.ReadLine());
        }

        #endregion

        private static void Analyze(string data)
        {
            if (data.Length == 0)
            {
                Logger.Error("Empty data string.");
            }
            else
            {
                try
                {
                    byte[] bytes = data.Split('-').Select(part => Convert.ToByte(part, 16)).ToArray();
                    Logger.Debug("Analyzing {0} bytes ...", bytes.Length);
                    Logger.Debug(String.Empty);
                    AnalyzeMessage(bytes, 0);
                }
                catch (FormatException ex)
                {
                    Logger.Error(ex.Message);
                }
            }
        }

        private static void AnalyzeMessage(byte[] data, int depth)
        {
            using (Stream stream = new MemoryStream(data))
            {
                AnalyzeMessage(stream, depth);
            }
        }

        private static string FormatDepth(int depth)
        {
            return new String(' ', depth * 4);
        }

        private static void AnalyzeMessage(Stream stream, int depth)
        {
            while (true)
            {
                WireType wireType;
                uint fieldNumber;
                try
                {
                    ProtobufReader.ReadKey(stream, out fieldNumber, out wireType);
                }
                catch (EndOfStreamException)
                {
                    break;
                }
                Logger.Info("{2}{0} : {1}", fieldNumber, wireType,
                    FormatDepth(depth));
                Action<Stream, int> analyzer;
                if (!Analyzers.TryGetValue(wireType, out analyzer))
                {
                    Logger.Error("{0}Invalid wiretype - not a message?",
                        FormatDepth(depth + 1));
                    break;
                }
                analyzer(stream, depth + 1);
            }
        }

        #region Analyzers

        private static void AnalyzeLengthDelimited(Stream stream, int depth)
        {
            byte[] data = ProtobufReader.ReadBytes(stream);
            if (data.Length == 0)
            {
                Logger.Info("{0}Empty.", FormatDepth(depth));
                return;
            }
            Logger.Info("{2}As byte[{0}] = {1}", data.Length, BitConverter.ToString(data),
                FormatDepth(depth));
            Logger.Info("{1}As String: {0}", CleanUpString(
                System.Text.Encoding.UTF8.GetString(data)),
                FormatDepth(depth));
            if (data.Length == 4)
            {
                IPAddress ipAddress = new IPAddress(data);
                Logger.Info("{1}As IP address: {0}", ipAddress, FormatDepth(depth));
            }
            if (data.Length == 16)
            {
                Guid guid = new Guid(data);
                Logger.Info("{1}As GUID: {0}", guid, FormatDepth(depth));
            }
            Logger.Info("{0}As message:", FormatDepth(depth));
            Logger.Info("{0}{{", FormatDepth(depth));
            AnalyzeMessage(data, depth + 1);
            Logger.Info("{0}}}", FormatDepth(depth));
        }

        private static void AnalyzeVarint(Stream stream, int depth)
        {
            ulong value = ProtobufReader.ReadUnsignedVarint(stream);
            Logger.Info("{1}As unsigned: {0}", value, FormatDepth(depth));
            ulong mask = 0L - (value & 1);
            long signedValue = (long)(value >> 1 ^ mask);
            Logger.Info("{1}As signed: {0}", signedValue, FormatDepth(depth));
        }

        private static void AnalyzeFixed32(Stream stream, int depth)
        {
            byte[] data = ProtobufReader.ReadRawBytes(stream, 4);
            Logger.Info("{1}As Int32: {0}", BitConverter.ToInt32(data, 0), FormatDepth(depth));
            Logger.Info("{1}As UInt32: {0}", BitConverter.ToUInt32(data, 0), FormatDepth(depth));
            Logger.Info("{1}As Float: {0}", BitConverter.ToSingle(data, 0), FormatDepth(depth));
        }

        private static void AnalyzeFixed64(Stream stream, int depth)
        {
            byte[] data = ProtobufReader.ReadRawBytes(stream, 8);
            Logger.Info("{1}As Int64: {0}", BitConverter.ToInt64(data, 0), FormatDepth(depth));
            Logger.Info("{1}As UInt64: {0}", BitConverter.ToUInt64(data, 0), FormatDepth(depth));
            Logger.Info("{1}As Double: {0}", BitConverter.ToDouble(data, 0), FormatDepth(depth));
        }

        #endregion

        #region Helpers

        private static string CleanUpString(string value)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                char ch = value[i];
                int b = ch;
                result.Append(b < 32 ? '.' : ch);
            }
            return result.ToString();
        }

        #endregion
    }
}
