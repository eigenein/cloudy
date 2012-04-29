using System;
using System.Globalization;
using Cloudy.Computing.Topologies.Enums;
using Cloudy.Computing.Topologies.Interfaces;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Topologies.Structures
{
    /// <summary>
    /// Represents a thread rank in the star topology.
    /// </summary>
    [ProtobufSerializable]
    public class StarRank : IRank
    {
        /// <summary>
        /// Empty constructor for deserialization.
        /// </summary>
        public StarRank()
        {
            // Do nothing.
        }

        public StarRank(int index)
        {
            this.Index = index;
        }

        public static readonly StarRank Central = new StarRank(0);

        /// <summary>
        /// Gets the index in a star. Zero is the central thread, while others
        /// are peripheral.
        /// </summary>
        [ProtobufField(1)]
        public int Index { get; set; }

        public bool IsCentral
        {
            get { return Index == 0; }
        }

        public override string ToString()
        {
            return Index.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            StarRank rank = obj as StarRank;
            if (rank == null)
            {
                return false;
            }
            return Index.Equals(rank.Index);
        }

        public bool Equals(StarRank obj)
        {
            if (obj == null)
            {
                return false;
            }
            return Index.Equals(obj.Index);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }

        #region Implementation of IRank

        public TopologyType TopologyType
        {
            get { return TopologyType.Star; }
        }

        #endregion
    }
}
