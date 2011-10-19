using System;
using Cloudy.Protobuf.Enums;

namespace Cloudy.Protobuf.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
    public class ProtobufFieldAttribute : Attribute
    {
        private readonly uint fieldNumber;

        private readonly bool packed;

        private readonly bool required;

        private readonly DataType dataType;

        public ProtobufFieldAttribute(uint fieldNumber, 
            DataType dataType = DataType.Default,
            bool packed = false, bool required = false)
        {
            this.fieldNumber = fieldNumber;
            this.packed = packed;
            this.required = required;
            this.dataType = dataType;
        }

        public uint FieldNumber
        {
            get { return fieldNumber; }
        }

        public bool Packed
        {
            get { return packed; }
        }

        public bool Required
        {
            get { return required; }
        }

        public DataType DataType
        {
            get { return dataType; }
        }
    }
}
