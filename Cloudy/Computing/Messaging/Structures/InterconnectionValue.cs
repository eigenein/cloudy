using System;
using System.Net;
using Cloudy.Computing.Structures;
using Cloudy.Computing.Topologies.Structures;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Messaging.Structures
{
    [ProtobufSerializable]
    public class InterconnectionValue
    {
        /// <summary>
        /// Empty constructor for deserialization.
        /// </summary>
        public InterconnectionValue()
        {
            // Do nothing.
        }

        [ProtobufField(1)]
        public ThreadAddress LocalThreadAddress { get; set; }

        [ProtobufField(2)]
        public ThreadAddress RemoteThreadAddress { get; set; }

        [ProtobufField(3)]
        public byte[] LocalAddress { get; set; }

        [ProtobufField(4)]
        public int LocalPort { get; set; }

        [ProtobufField(5)]
        public byte[] ExternalAddress { get; set; }

        [ProtobufField(6)]
        public int ExternalPort { get; set; }

        public IPEndPoint LocalEndPoint
        {
            get { return new IPEndPoint(new IPAddress(LocalAddress), LocalPort); }
            set
            {
                LocalAddress = value.Address.GetAddressBytes();
                LocalPort = value.Port;
            }
        }

        public IPEndPoint ExternalEndPoint
        {
            get { return new IPEndPoint(new IPAddress(ExternalAddress), ExternalPort); }
            set
            {
                ExternalAddress = value.Address.GetAddressBytes();
                ExternalPort = value.Port;
            }
        }

        public override string ToString()
        {
            return String.Format("Thread: {2}, endpoints: {0}, {1}", 
                LocalEndPoint, ExternalEndPoint, RemoteThreadAddress);
        }
    }
}
