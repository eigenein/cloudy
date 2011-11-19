using System;
using System.Linq;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Topologies.Structures
{
    /// <summary>
    /// Represents the thread address.
    /// </summary>
    [ProtobufSerializable]
    public class ThreadAddress
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

        public override int GetHashCode()
        {
            return Parts.Aggregate(0, (current, part) => current * 31 + part);
        }

        public override string ToString()
        {
            return String.Join(":", Parts.Select(part => part.ToString()).ToArray());
        }
    }
}
