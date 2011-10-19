using System;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.Structures
{
    public class BuildingSerializer
    {
        private WireTypedSerializer serializer;

        private IValueBuilder builder;

        public BuildingSerializer(WireTypedSerializer serializer,
            IValueBuilder builder)
        {
            this.serializer = serializer;
            this.builder = builder;
        }

        public WireTypedSerializer Serializer
        {
            get { return serializer; }
            set { serializer = value; }
        }

        public IValueBuilder Builder
        {
            get { return builder; }
            set { builder = value; }
        }
    }
}
