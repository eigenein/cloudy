using System;
using Cloudy.Computing.Topologies.Structures;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Messaging.Structures
{
    [ProtobufSerializable]
    public class AllocateThreadValue
    {
        [ProtobufField(1)]
        public ThreadAddress ThreadAddress { get; set; }
    }
}
