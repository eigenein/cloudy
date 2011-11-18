using System;
using System.Net;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Messaging.Structures
{
    [ProtobufSerializable]
    public class JoinRequestValue
    {
        /// <summary>
        /// Empty constructor for deserialization.
        /// </summary>
        public JoinRequestValue()
        {
            // Do nothing.
        }

        public JoinRequestValue(IPEndPoint localEndPoint, int slotsCount, byte[] metadata)
        {
            this.LocalAddress = localEndPoint.Address.GetAddressBytes();
            this.LocalPort = localEndPoint.Port;
            this.SlotsCount = slotsCount;
        }

        [ProtobufField(1)]
        public byte[] LocalAddress { get; set; }

        [ProtobufField(2)]
        public int LocalPort { get; set; }

        [ProtobufField(3)]
        public byte[] Metadata { get; set; }

        [ProtobufField(4)]
        public int SlotsCount { get; set; }

        public IPEndPoint LocalEndPoint
        {
            get { return new IPEndPoint(new IPAddress(LocalAddress), LocalPort); }
        }
    }
}
