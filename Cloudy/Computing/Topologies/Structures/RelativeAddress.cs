using System;
using System.Linq;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Topologies.Structures
{
    [ProtobufSerializable]
    public class RelativeAddress
    {
        [ProtobufField(1)]
        public int[] Parts { get; set; }

        public int Length
        {
            get { return Parts.Length; }
        }

        public int this[int index]
        {
            get { return Parts[index]; }
        }

        public override string ToString()
        {
            return String.Join(":", Parts.Select(part => part.ToString("8X")).ToArray());
        }
    }
}
