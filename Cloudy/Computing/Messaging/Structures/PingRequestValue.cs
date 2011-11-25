using System;
using System.Net;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Messaging.Structures
{
    [ProtobufSerializable]
    public class PingRequestValue
    {
        [ProtobufField(1)]
        public byte[] SourceAddress { get; set; }

        [ProtobufField(2)]
        public int SourcePort { get; set; }

        [ProtobufField(3)]
        public byte[] TargetAddress { get; set; }

        [ProtobufField(4)]
        public int TargetPort { get; set; }

        public IPEndPoint SourceEndPoint
        {
            get { return new IPEndPoint(new IPAddress(SourceAddress), SourcePort); }
            set
            {
                SourceAddress = value.Address.GetAddressBytes();
                SourcePort = value.Port;
            }
        }

        public IPEndPoint TargetEndPoint
        {
            get { return new IPEndPoint(new IPAddress(TargetAddress), TargetPort); }
            set
            {
                TargetAddress = value.Address.GetAddressBytes();
                TargetPort = value.Port;
            }
        }
    }
}
