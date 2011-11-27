using System;
using System.Net;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Structures.Values
{
    [ProtobufSerializable]
    public class EndPointValue
    {
        [ProtobufField(1)]
        public int Port { get; set; }

        [ProtobufField(2)]
        public byte[] Address { get; set; }

        public IPEndPoint Value
        {
            get { return new IPEndPoint(new IPAddress(Address), Port); }
            set
            {
                Port = value.Port;
                Address = value.Address.GetAddressBytes();
            }
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
