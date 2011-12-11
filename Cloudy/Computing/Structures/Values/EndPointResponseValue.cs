using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Structures.Values
{
    [ProtobufSerializable]
    public class EndPointResponseValue
    {
        public EndPointResponseValue()
        {
            this.LocalEndPoint = new EndPointValue();
            this.ExternalEndPoint = new EndPointValue();
        }

        [ProtobufField(1)]
        public EndPointValue LocalEndPoint { get; set; }

        [ProtobufField(2)]
        public EndPointValue ExternalEndPoint { get; set; }

        public override string ToString()
        {
            return String.Format("{0}, {1}", LocalEndPoint, ExternalEndPoint);
        }
    }
}
