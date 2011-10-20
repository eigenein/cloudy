using System;
using System.Collections.Generic;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.ValueBuilders
{
    public class RepeatedValueBuilder : IValueBuilder
    {
        private List<object> objects;

        private readonly bool instantiateEmptyList;

        public RepeatedValueBuilder(bool instantiateEmptyList)
        {
            this.instantiateEmptyList = instantiateEmptyList;
        }

        #region Implementation of IValueBuilder

        public IValueBuilder CreateInstance()
        {
            return new RepeatedValueBuilder(instantiateEmptyList);
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
            return objects ?? (instantiateEmptyList ? new List<object>() : null);
        }

        #endregion
    }
}
