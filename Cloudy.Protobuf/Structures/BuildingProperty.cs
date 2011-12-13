using System;
using System.Reflection;

namespace Cloudy.Protobuf.Structures
{
    /// <summary>
    /// Combines the property and the related building serializer.
    /// </summary>
    public class BuildingProperty
    {
        private readonly PropertyInfo property;

        private readonly BuildingSerializer buildingSerializer;

        public BuildingProperty(PropertyInfo property,
            BuildingSerializer buildingSerializer)
        {
            this.property = property;
            this.buildingSerializer = buildingSerializer;
        }

        public PropertyInfo Property
        {
            get { return property; }
        }

        public BuildingSerializer BuildingSerializer
        {
            get { return buildingSerializer; }
        }
    }
}
