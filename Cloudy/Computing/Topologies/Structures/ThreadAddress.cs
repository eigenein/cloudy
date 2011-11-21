using System;
using System.Collections.Generic;
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
        private int[] parts;

        /// <summary>
        /// Empty constructor for deserialization.
        /// </summary>
        public ThreadAddress()
        {
            // Do nothing.
        }

        public ThreadAddress(int value)
        {
            Parts = new[] { value };
        }

        public static ThreadAddress EmptySingleton = new ThreadAddress(0);

        [ProtobufField(1)]
        public ICollection<int> Parts
        {
            get { return parts; }
            set { this.parts = value.ToArray(); }
        }

        public int Length
        {
            get { return parts.Length; }
        }

        public int this[int index]
        {
            get { return parts[index]; }
        }

        public override bool Equals(object obj)
        {
            ThreadAddress other = obj as ThreadAddress;
            if (other == null)
            {
                return false;
            }
            return !parts.Where((part, i) => part != other.parts[i]).Any();
        }

        public override int GetHashCode()
        {
            return Parts.Aggregate(0, (current, part) => current * 31 + part);
        }

        public override string ToString()
        {
            return String.Join(":", parts.Select(part => part.ToString()).ToArray());
        }
    }
}
