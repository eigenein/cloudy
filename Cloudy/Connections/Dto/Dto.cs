using System;
using Cloudy.Protobuf.Attributes;
using Cloudy.Protobuf.Enums;

namespace Cloudy.Connections.Dto
{
    [ProtobufSerializable]
    public class Dto<T>
    {
        private readonly uint? tag;

        private readonly T value;

        private readonly uint type;

        protected Dto(uint? tag, uint type, T value)
        {
            this.tag = tag;
            this.value = value;
            this.type = type;
        }

        [ProtobufField(1)]
        public uint? Tag
        {
            get { return tag; }
        }

        [ProtobufField(2)]
        public uint Type
        { 
            get { return type; }
        }

        [ProtobufField(3)]
        public T Value
        { 
            get { return value; }
        }

        public bool IsResponse
        {
            get { return tag != null; }
        }
    }
}
