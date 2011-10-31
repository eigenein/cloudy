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

        /// <summary>
        /// Creates a serializer of objects of the specified type.
        /// </summary>
        public static Serializer CreateSerializer(Type type)
        {
            lock (SerializerCache)
            {
                return CreateSerializerThreadUnsafe(type);
            }
        }

        private static Serializer CreateSerializerThreadUnsafe(Type type)
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
                    new NullProxySerializer(buildingProperty.BuildingSerializer.Serializer,
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
            BuildingSerializer buildingSerializer;
            if (TrySerializeAsPrimitiveValue(propertyType, attribute, out buildingSerializer))
            {
                return buildingSerializer;
            }
            if (TrySerializeAsEnum(propertyType, out buildingSerializer))
            {
                return buildingSerializer;
            }
            if (TrySerializeAsNullable(propertyType, attribute, out buildingSerializer))
            {
                return buildingSerializer;
            }
            if (TrySerializeAsCollection(propertyType, attribute, examineForCollection,
                out buildingSerializer))
            {
                return buildingSerializer;
            }
            return new BuildingSerializer(new EmbeddedMessageSerializer(
                CreateSerializer(propertyType)),
                new SingleValueBuilder(propertyType));
        }

        private static bool TrySerializeAsNullable(Type propertyType,
            ProtobufFieldAttribute attribute, out BuildingSerializer buildingSerializer)
        {
            buildingSerializer = null;
            if (propertyType.IsGenericType &&
                propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Type underlyingType = propertyType.GetGenericArguments()[0];
                SerializerWithWireType underlyingSerializer = CreateBuildingSerializer(
                    underlyingType, attribute, false).Serializer;
                buildingSerializer = new BuildingSerializer(
                    underlyingSerializer, new NullableValueBuilder(underlyingType));
                return true;
            }
            return false;
        }

        private static bool TrySerializeAsEnum(Type propertyType,
            out BuildingSerializer buildingSerializer)
        {
            buildingSerializer = null;
            if (propertyType.IsSubclassOf(typeof(Enum)))
            {
                buildingSerializer = new BuildingSerializer(
                    new EnumSerializer(propertyType),
                    new SingleValueBuilder(propertyType));
                return true;
            }
            return false;
        }

        private static bool TrySerializeAsCollection(Type propertyType, 
            ProtobufFieldAttribute attribute, bool examineForCollection, 
            out BuildingSerializer buildingSerializer)
        {
            buildingSerializer = null;
            if (examineForCollection &&
                propertyType.IsGenericType &&
                    propertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
            {
                Type underlyingType = propertyType.GetGenericArguments()[0];
                SerializerWithWireType underlyingSerializer = CreateBuildingSerializer(
                    underlyingType, attribute, false).Serializer;
                {
                    buildingSerializer = new BuildingSerializer(attribute.Packed
                        ? (SerializerWithWireType)new PackedRepeatedSerializer(underlyingSerializer)
                        : new RepeatedSerializer(attribute.FieldNumber, underlyingSerializer),
                        new RepeatedValueBuilder(underlyingType, !attribute.Packed));
                    return true;
                }
            }
            return false;
        }

        private static bool TrySerializeAsPrimitiveValue(Type propertyType, 
            ProtobufFieldAttribute attribute,
            out BuildingSerializer buildingSerializer)
        {
            buildingSerializer = null;
            SerializerWithWireType serializer;
            if ((attribute.DataType != DataType.Default &&
                DataTypeToSerializerCache.TryGetSerializer(attribute.DataType, out serializer))
                    || DefaultSerializersCache.TryGetSerializer(propertyType, out serializer))
            {
                {
                    buildingSerializer = new BuildingSerializer(serializer,
                        new SingleValueBuilder(propertyType));
                    return true;
                }
            }
            return false;
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
                SerializerWithWireType serializer = entry.Value.BuildingSerializer.Serializer;
                object value = entry.Value.Property.GetValue(o, null);
                if (!serializer.ShouldBeSkipped(value))
                {
                    ProtobufWriter.WriteKey(stream, entry.Key, serializer.WireType);
                    entry.Value.BuildingSerializer.Serializer.Serialize(stream, value);
                }
            }
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <param name="stream">The target stream.</param>
        /// <param name="o">The object to serialize.</param>
        /// <param name="streamingMode">Tells whether the object should be
        /// serialized as an embedded message.</param>
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
                WireType wireType;
                uint fieldNumber;
                try
                {
                    ProtobufReader.ReadKey(stream, out fieldNumber, out wireType);
                }
                catch (EndOfStreamException)
                {
                    break;
                }
                IValueBuilder valueBuilder;
                if (buildingProperties.TryGetValue(fieldNumber, out valueBuilder))
                {
                    valueBuilder.UpdateValue(properties[fieldNumber]
                        .BuildingSerializer.Serializer.Deserialize(stream));
                }
                else
                {
                    UnknownFieldSkipHelper.Skip(stream, wireType);
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
