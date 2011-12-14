using System;
using Cloudy.Computing.Topologies.Enums;
using Cloudy.Computing.Topologies.Interfaces;
using Cloudy.Protobuf.Attributes;

namespace Cloudy.Computing.Topologies.Structures
{
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
            return Index.Equals(obj);
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
