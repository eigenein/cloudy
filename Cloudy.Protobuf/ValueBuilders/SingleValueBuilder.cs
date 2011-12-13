using System;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.ValueBuilders
{
    /// <summary>
    /// The simplest value builder - remembers the last value set as this
    /// is required by the Protocol Buffers specification for 
    /// primitive types.
    /// </summary>
    public class SingleValueBuilder : IValueBuilder
    {
        private readonly Type expectedType;

        private object result;

        public SingleValueBuilder(Type expectedType)
        {
            this.expectedType = expectedType;
        }

        #region Implementation of IValueBuilder

        public IValueBuilder CreateInstance()
        {
            return new SingleValueBuilder(expectedType);
        }

        public void UpdateValue(object o)
        {
            if (o.GetType() != expectedType)
            {
                throw new InvalidOperationException(String.Format(
                    "Expected type: {0}, but was: {1}", expectedType, o.GetType()));
            }
            result = o;
        }

        public object BuildObject()
        {
            return result;
        }

        #endregion
    }
}
