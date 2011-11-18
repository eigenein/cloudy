using System;
using System.Net;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Messaging.Structures
{
    [ProtobufSerializable]
    public class JoinResponseValue
    {
        /// <summary>
        /// Empty constructor for deserialization.
        /// </summary>
        public JoinResponseValue()
        {
            // Do nothing.
        }

        public JoinResponseValue(IPEndPoint externalEndPoint)
        {
            this.ExternalAddress = externalEndPoint.Address.GetAddressBytes();
            this.ExternalPort = externalEndPoint.Port;
        }

        [ProtobufField(1)]
        public byte[] ExternalAddress { get; set; }

        [ProtobufField(2)]
        public int ExternalPort { get; set; }

        public IPEndPoint ExternalEndPoint
        {
            get { return new IPEndPoint(new IPAddress(ExternalAddress), ExternalPort); }
        }
    }
}
