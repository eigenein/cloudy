﻿using System;
using System.IO;
using Cloudy.Protobuf.Enums;
using Cloudy.Protobuf.Exceptions;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.Serializers
{
    public class CheckNullSerializer : WireTypedSerializer
    {
        private readonly WireTypedSerializer serializer;

        private readonly bool allowNull;

        public CheckNullSerializer(WireTypedSerializer serializer,
            bool allowNull)
        {
            this.serializer = serializer;
            this.allowNull = allowNull;
        }

        #region Overrides of AbstractSerializer

        public override void Serialize(Stream stream, object o)
        {
            if (o != null)
            {
                serializer.Serialize(stream, o);
            }
            else if (!allowNull)
            {
                throw new MissingValueException("A field is not allowed to be null.");
            }
        }

        public override object Deserialize(Stream stream)
        {
            return serializer.Deserialize(stream);
        }

        public override bool ShouldBeSkipped(object value)
        {
            return serializer.ShouldBeSkipped(value) || (value == null && allowNull);
        }

        #endregion

        #region Overrides of WireTypedSerializer

        public override WireType WireType
        {
            get { return serializer.WireType; }
        }

        #endregion
    }
}