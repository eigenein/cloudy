using System;
using System.Net;
using Cloudy.Computing.Structures;
using Cloudy.Computing.Topologies.Structures;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Messaging.Structures
{
    [ProtobufSerializable]
    public class NeighborValue
    {
        /// <summary>
        /// Empty constructor for deserialization.
        /// </summary>
        public NeighborValue()
        {
            // Do nothing.
        }

        public NeighborValue(ThreadAddress threadAddress, SlaveContext slaveContext)
        {
            this.ThreadAddress = threadAddress;
            this.LocalPort = slaveContext.LocalEndPoint.Port;
            this.LocalAddress = slaveContext.LocalEndPoint.Address.GetAddressBytes();
            this.ExternalPort = slaveContext.ExternalEndPoint.Port;
            this.ExternalAddress = slaveContext.ExternalEndPoint.Address.GetAddressBytes();
        }

        [ProtobufField(1)]
        public ThreadAddress ThreadAddress { get; set; }

        [ProtobufField(2)]
        public byte[] LocalAddress { get; set; }

        [ProtobufField(3)]
        public int LocalPort { get; set; }

        [ProtobufField(4)]
        public byte[] ExternalAddress { get; set; }

        [ProtobufField(5)]
        public int ExternalPort { get; set; }

        public IPEndPoint LocalEndPoint
        {
            get { return new IPEndPoint(new IPAddress(LocalAddress), LocalPort); }
        }

        public IPEndPoint ExternalEndPoint
        {
            get { return new IPEndPoint(new IPAddress(ExternalAddress), ExternalPort); }
        }

        public override string ToString()
        {
            return String.Format("Thread: {2}, endpoints: {0}, {1}", 
                LocalEndPoint, ExternalEndPoint, ThreadAddress);
        }
    }
}
