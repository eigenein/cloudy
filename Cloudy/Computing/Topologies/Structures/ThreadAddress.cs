using System;
using System.Linq;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Topologies.Structures
{
    /// <summary>
    /// Represents a slave address within a network with multiple topologies.
    /// </summary>
    [ProtobufSerializable]
    public class ThreadAddress
    {
        /// <summary>
        /// Gets or sets the topology index.
        /// </summary>
        [ProtobufField(1)]
        public int TopologyIndex { get; set; }

        /// <summary>
        /// Gets or sets the thread address relatively to the topology index.
        /// </summary>
        [ProtobufField(2)]
        public int[] RelativeAddress { get; set; }

        public override int GetHashCode()
        {
            return RelativeAddress.Aggregate(TopologyIndex, (current, value) => current * 31 + value);
        }

        public override bool Equals(object obj)
        {
            ThreadAddress anotherAddress = obj as ThreadAddress;
            if (anotherAddress == null || 
                anotherAddress.TopologyIndex != TopologyIndex ||
                anotherAddress.RelativeAddress.Length != RelativeAddress.Length)
            {
                return false;
            }
            return !RelativeAddress.Where((el, i) => el != anotherAddress.RelativeAddress[i]).Any();
        }
    }
}
