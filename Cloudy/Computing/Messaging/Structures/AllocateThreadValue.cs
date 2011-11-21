using System;
using Cloudy.Computing.Topologies.Enums;
using Cloudy.Computing.Topologies.Structures;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Messaging.Structures
{
    [ProtobufSerializable]
    public class AllocateThreadValue
    {
        [ProtobufField(1)]
        public ThreadAddress ThreadAddress { get; set; }

        [ProtobufField(2)]
        public TopologyType TopologyType { get; set; }

        public override string ToString()
        {
            return String.Format("{0}@{1}", ThreadAddress, TopologyType);
        }
    }
}
