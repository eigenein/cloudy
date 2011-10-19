using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cloudy.Protobuf.Attributes;
using Cloudy.Protobuf.Encoding;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Exceptions;
using Cloudy.Protobuf.Helpers;
using Cloudy.Protobuf.Interfaces;
using Cloudy.Protobuf.Serializers;
using Cloudy.Protobuf.Structures;
using Cloudy.Protobuf.ValueBuilders;

namespace Cloudy.Protobuf
{
    /// <summary>
    /// Used to serialize to and deserialize from the Protobuf format.
    /// </summary>
    public class Serializer : AbstractSerializer
    {
        private readonly Type expectedType;

        private readonly Dictionary<uint, BuildingProperty> properties;

        private Serializer(Type expectedType, Dictionary<uint, BuildingProperty> properties)
        {
            this.expectedType = expectedType;
            this.properties = properties;
        }

        private static readonly Dictionary<Type, Serializer> SerializerCache =
            new Dictionary<Type, Serializer>();

        public static Serializer CreateSerializer(Type type)
        {
            Serializer serializer;
            if (SerializerCache.TryGetValue(type, out serializer))
            {
                return serializer;
            }
            ProtobufSerializableAttribute serializableAttribute =
                (ProtobufSerializableAttribute)Attribute.GetCustomAttribute(
                type, typeof(ProtobufSerializableAttribute));
            if (serializableAttribute == null)
            {
                throw new NotSerializableException(type);
            }
            Dictionary<uint, BuildingProperty> properties =
                new Dictionary<uint, BuildingProperty>();
            foreach (PropertyInfo property in type.GetProperties())
            {
                ProtobufFieldAttribute fieldAttribute = 
                    (ProtobufFieldAttribute)Attribute.GetCustomAttribute(
                    property, typeof(ProtobufFieldAttribute));
                if (fieldAttribute == null)
                {
                    continue;
                }
                if (properties.ContainsKey(fieldAttribute.FieldNumber))
                {
                    throw new DuplicateFieldNumberException(fieldAttribute.FieldNumber);
                }
                BuildingProperty buildingProperty = new BuildingProperty(
                    property, CreateBuildingSerializer(property, fieldAttribute));
                buildingProperty.BuildingSerializer.Serializer =
                    new CheckNullSerializer(buildingProperty.BuildingSerializer.Serializer,
                        !fieldAttribute.Required);
                if (fieldAttribute.Required)
                {
                    buildingProperty.BuildingSerializer.Builder =
                        new RequiredValueBuilder(buildingProperty.BuildingSerializer.Builder);
                }
                properties[fieldAttribute.FieldNumber] = buildingProperty;
            }
            return SerializerCache[type] = new Serializer(type, properties);
        }

        private static BuildingSerializer CreateBuildingSerializer(PropertyInfo property,
            ProtobufFieldAttribute attribute)
        {
            WireTypedSerializer serializer;
            // Trying to serialize as a single value first.
            if ((attribute.DataType != DataType.Default &&
                DataTypeToSerializerCache.TryGetSerializer(attribute.DataType, out serializer))
                || DefaultSerializersCache.TryGetSerializer(property.PropertyType, out serializer))
            {
                return new BuildingSerializer(serializer,
                    new SingleValueBuilder(property.PropertyType));
            }
            // Trying to serialize as a collection.
            // TODO:
            // trying to serialize as an embedded message.
            return new BuildingSerializer(new EmbeddedMessageSerializer(
                CreateSerializer(property.PropertyType)),
                new SingleValueBuilder(property.PropertyType));
        }

        public override void Serialize(Stream stream, object o)
        {
            if (o.GetType() != expectedType)
            {
                throw new InvalidOperationException(String.Format(
                    "Expected type: {0}, but was: {1}", expectedType, o.GetType()));
            }
            foreach (KeyValuePair<uint, BuildingProperty> entry in properties)
            {
                WireTypedSerializer serializer = entry.Value.BuildingSerializer.Serializer;
                object value = entry.Value.Property.GetValue(o, null);
                if (!serializer.ShouldBeSkipped(value))
                {
                    ProtobufWriter.WriteKey(stream, entry.Key, serializer.WireType);
                    entry.Value.BuildingSerializer.Serializer.Serialize(stream, value);
                }
            }
        }

        public override object Deserialize(Stream stream)
        {
            //Dictionary<int, IValueBuilder> buildingProperties = properties.ToDictionary(
            //    entry => entry.Key,
            //    entry => entry.Value.BuildingSerializer.Builder.CreateInstance());
            return null;
        }
    }
}
