using System;
using System.Collections.Generic;
using System.Reflection;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.ValueBuilders
{
    public class NullableValueBuilder : IValueBuilder
    {
        private readonly Type underlyingType;

        private object result;

        private static readonly Dictionary<Type, MethodInfo> CastMethodsCache =
            new Dictionary<Type, MethodInfo>();

        public NullableValueBuilder(Type underlyingType)
        {
            this.underlyingType = underlyingType;
        }

        #region Implementation of IValueBuilder

        /// <summary>
        /// Creates a new instance of the builder.
        /// </summary>
        public IValueBuilder CreateInstance()
        {
            return new NullableValueBuilder(underlyingType);
        }

        /// <summary>
        /// Called when the value is read from the input stream.
        /// </summary>
        public void UpdateValue(object o)
        {
            result = o;
        }

        /// <summary>
        /// Called to get a final value to set into a deserialized object.
        /// </summary>
        public object BuildObject()
        {
            if (result == null)
            {
                return null;
            }
            MethodInfo castMethod;
            if (!CastMethodsCache.TryGetValue(underlyingType, out castMethod))
            {
                CastMethodsCache[underlyingType] = castMethod =
                    GetType().GetMethod("Cast").MakeGenericMethod(underlyingType);
            }
            return castMethod.Invoke(null, new object[] { result });
        }

        #endregion

        public static T? Cast<T>(T o)
            where T : struct
        {
            return o;
        }
    }
}
