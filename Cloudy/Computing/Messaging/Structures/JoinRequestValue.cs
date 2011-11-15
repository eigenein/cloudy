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

        public JoinRequestValue(IPEndPoint localEndPoint, byte[] metadata)
        {
            this.LocalAddress = localEndPoint.Address.GetAddressBytes();
            this.LocalPort = localEndPoint.Port;
        }

        [ProtobufField(1)]
        public byte[] LocalAddress { get; set; }

        [ProtobufField(2)]
        public int LocalPort { get; set; }

        [ProtobufField(3)]
        public byte[] Metadata { get; set; }

        public IPEndPoint LocalEndPoint
        {
            get { return new IPEndPoint(new IPAddress(LocalAddress), LocalPort); }
        }
    }
}
