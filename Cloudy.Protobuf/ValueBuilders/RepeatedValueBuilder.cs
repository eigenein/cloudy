using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.ValueBuilders
{
    /// <summary>
    /// Used to construct a collection of values from repeated single values.
    /// </summary>
    public class RepeatedValueBuilder : IValueBuilder
    {
        private List<object> objects;

        private readonly bool instantiateEmptyList;

        private readonly Type underlyingType;

        private static readonly Dictionary<Type, MethodInfo> CastMethodsCache =
            new Dictionary<Type, MethodInfo>();

        public RepeatedValueBuilder(Type underlyingType, bool instantiateEmptyList)
        {
            this.instantiateEmptyList = instantiateEmptyList;
            this.underlyingType = underlyingType;
        }

        #region Implementation of IValueBuilder

        public IValueBuilder CreateInstance()
        {
            return new RepeatedValueBuilder(underlyingType, instantiateEmptyList);
        }

        public void UpdateValue(object o)
        {
            if (objects == null)
            {
                objects = new List<object>();
            }
            objects.AddRange((List<object>)o);
        }

        public object BuildObject()
        {
            List<object> list = objects ?? (instantiateEmptyList ? new List<object>() : null);
            if (list == null)
            {
                return null;
            }
            MethodInfo castMethod;
            if (!CastMethodsCache.TryGetValue(underlyingType, out castMethod))
            {
                CastMethodsCache[underlyingType] = castMethod =
                    GetType().GetMethod("Cast").MakeGenericMethod(underlyingType);
            }
            return castMethod.Invoke(null, new object[] { list });
        }

        #endregion

        public static ICollection<T> Cast<T>(ICollection<object> objects)
        {
            return objects.Select(o => (T)o).ToList();
        }
    }
}
