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

        /// <summary>
        /// Indicates whether the requested thread was found.
        /// </summary>
        /// <value>The <c>null</c> means the same as <c>true</c>.</value>
        [ProtobufField(3)]
        public bool? IsFound { get; set; }

        public override string ToString()
        {
            return String.Format("{0}, {1}", LocalEndPoint, ExternalEndPoint);
        }
    }
}
