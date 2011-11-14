using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Topologies.Structures
{
    /// <summary>
    /// Uniquely identifies a thread location in the whole network.
    /// </summary>
    [ProtobufSerializable]
    public class ThreadAddress
    {
        /// <summary>
        /// Identifies one of active topologies in the current network.
        /// </summary>
        [ProtobufField(1)]
        public int TopologyId { get; set; }
    }

    /// <summary>
    /// Uniquely identifies a thread location in the whole network.
    /// </summary>
    [ProtobufSerializable]
    public class ThreadAddress<TAddress> : ThreadAddress
    {
        /// <summary>
        /// Identifies a node location within the topology.
        /// </summary>
        [ProtobufField(2)]
        public TAddress Address { get; set; }
    }
}
