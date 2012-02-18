using System;
using Cloudy.Helpers;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Structures.Values
{
    [ProtobufSerializable]
    public class GetRemoteValueRequest
    {
        public GetRemoteValueRequest()
        {
            this.Namespace = Namespaces.Default;
        }

        [ProtobufField(1)]
        public byte[] Namespace { get; set; }

        [ProtobufField(2)]
        public string Key { get; set; }
    }
}
