using System;

using Cloudy.Computing.Structures.Values;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Test.Protobuf.TestClasses
{
    [ProtobufSerializable]
    public class K
    {
        public K()
        {
            this.LocalEndPoint = new EndPointValue();
            this.ExternalEndPoint = new EndPointValue();
        }

        [ProtobufField(1)]
        public EndPointValue LocalEndPoint { get; set; }

        [ProtobufField(2)]
        public EndPointValue ExternalEndPoint { get; set; }

        [ProtobufField(3)]
        public bool IsFound { get; set; }
    }
}
