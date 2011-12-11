﻿using System;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Structures.Values
{
    [ProtobufSerializable]
    public class SignedPingRequest
    {
        public SignedPingRequest()
        {
            this.SenderLocalEndPoint = new EndPointValue();
            this.SenderExternalEndPoint = new EndPointValue();
        }

        [ProtobufField(1)]
        public Guid SenderId { get; set; }

        [ProtobufField(2)]
        public EndPointValue SenderLocalEndPoint { get; set; }

        [ProtobufField(3)]
        public EndPointValue SenderExternalEndPoint { get; set; }

        [ProtobufField(4)]
        public Guid TargetId { get; set; }
    }
}
