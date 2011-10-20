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
                    property, CreateBuildingSerializer(property.PropertyType, fieldAttribute, true));
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

        private static BuildingSerializer CreateBuildingSerializer(Type propertyType,
            ProtobufFieldAttribute attribute, bool examineForCollection)
        {
            WireTypedSerializer serializer;
            // Trying to serialize as a single value first.
            if ((attribute.DataType != DataType.Default &&
                DataTypeToSerializerCache.TryGetSerializer(attribute.DataType, out serializer))
                || DefaultSerializersCache.TryGetSerializer(propertyType, out serializer))
            {
                return new BuildingSerializer(serializer,
                    new SingleValueBuilder(propertyType));
            }
            // Trying to serialize as a collection.
            if (examineForCollection &&
                propertyType.IsGenericType &&
                propertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
            {
                Type underlyingType = propertyType.GetGenericArguments()[0];
                WireTypedSerializer underlyingSerializer = CreateBuildingSerializer(
                    underlyingType, attribute, false).Serializer;
                return new BuildingSerializer(attribute.Packed ?
                    (WireTypedSerializer)new PackedRepeatedSerializer(underlyingSerializer) 
                        : new RepeatedSerializer(attribute.FieldNumber, underlyingSerializer),
                    new RepeatedValueBuilder(underlyingType, !attribute.Packed));
            }
            // Trying to serialize as an embedded message.
            return new BuildingSerializer(new EmbeddedMessageSerializer(
                CreateSerializer(propertyType)),
                new SingleValueBuilder(propertyType));
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

        public void Serialize(Stream stream, object o, bool streamingMode)
        {
            if (!streamingMode)
            {
                Serialize(stream, o);
            }
            else
            {
                new EmbeddedMessageSerializer(this).Serialize(stream, o);
            }
        }

        public override object Deserialize(Stream stream)
        {
            Dictionary<uint, IValueBuilder> buildingProperties = properties.ToDictionary(
                entry => entry.Key,
                entry => entry.Value.BuildingSerializer.Builder.CreateInstance());
            while (true)
            {
                try
                {
                    WireType wireType;
                    uint fieldNumber;
                    ProtobufReader.ReadKey(stream, out fieldNumber, out wireType);
                    IValueBuilder valueBuilder;
                    if (buildingProperties.TryGetValue(fieldNumber, out valueBuilder))
                    {
                        valueBuilder.UpdateValue(properties[fieldNumber]
                            .BuildingSerializer.Serializer.Deserialize(stream));
                    }
                }
                catch (EndOfStreamException)
                {
                    break;
                }
            }
            object o = Activator.CreateInstance(expectedType);
            foreach (KeyValuePair<uint, IValueBuilder> property in buildingProperties)
            {
                object value = property.Value.BuildObject();
                if (value != null)
                {
                    properties[property.Key].Property.SetValue(o,
                        value, null);
                }
            }
            return o;
        }

        public object Deserialize(Stream stream, bool streamingMode)
        {
            return streamingMode ? new EmbeddedMessageSerializer(this).Deserialize(stream)
                : Deserialize(stream);
        }
    }
}
