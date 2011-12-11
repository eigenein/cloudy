using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Structures.Values
{
    [ProtobufSerializable]
    public class SignedPingResponse
    {
        public SignedPingResponse()
        {
            this.SenderExternalEndPoint = new EndPointValue();
        }

        /// <summary>
        /// Success flag, <c>null</c> is the same as <c>false</c>.
        /// </summary>
        [ProtobufField(1)]
        public bool? Success { get; set; }

        [ProtobufField(2)]
        public EndPointValue SenderExternalEndPoint { get; set; }
    }
}
