using System;
using Cloudy.Protobuf.Enums;

namespace Cloudy.Protobuf.Attributes
{
    /// <summary>
    /// Marks a property as a serializable Protobuf field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
    public class ProtobufFieldAttribute : Attribute
    {
        private readonly uint fieldNumber;

        private readonly bool packed;

        private readonly bool required;

        private readonly DataType dataType;

        /// <summary>
        /// Initializes a new instance of the attribute.
        /// </summary>
        /// <param name="fieldNumber">The field number of the property in a target message.</param>
        /// <param name="dataType">Defines a way to serialize the property value.</param>
        /// <param name="packed">Defines whether a collection should be packed in a target message.</param>
        /// <param name="required">Marks the property as required in a target message.</param>
        public ProtobufFieldAttribute(uint fieldNumber, 
            DataType dataType = DataType.Default,
            bool packed = false, bool required = false)
        {
            this.fieldNumber = fieldNumber;
            this.packed = packed;
            this.required = required;
            this.dataType = dataType;
        }

        /// <summary>
        /// Gets the field number in a target message.
        /// </summary>
        public uint FieldNumber
        {
            get { return fieldNumber; }
        }

        /// <summary>
        /// Gets whether the field value is packed in a target message.
        /// </summary>
        public bool Packed
        {
            get { return packed; }
        }

        /// <summary>
        /// Gets whether the field value is required in a target message.
        /// </summary>
        public bool Required
        {
            get { return required; }
        }

        /// <summary>
        /// Gets a way to serialize the property value.
        /// </summary>
        public DataType DataType
        {
            get { return dataType; }
        }
    }
}
