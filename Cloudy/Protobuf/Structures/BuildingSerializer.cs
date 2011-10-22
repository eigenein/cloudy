using System;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.Structures
{
    /// <summary>
    /// Combines the serializer with the related value builder.
    /// </summary>
    public class BuildingSerializer
    {
        private SerializerWithWireType serializer;

        private IValueBuilder builder;

        public BuildingSerializer(SerializerWithWireType serializer,
            IValueBuilder builder)
        {
            this.serializer = serializer;
            this.builder = builder;
        }

        public SerializerWithWireType Serializer
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
