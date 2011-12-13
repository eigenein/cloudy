using System;
using Cloudy.Protobuf.Exceptions;
using Cloudy.Protobuf.Interfaces;

namespace Cloudy.Protobuf.ValueBuilders
{
    /// <summary>
    /// The proxy value builder - checks whether a required value is present.
    /// </summary>
    public class RequiredValueBuilder : IValueBuilder
    {
        private readonly IValueBuilder underlyingBuilder;

        public RequiredValueBuilder(IValueBuilder underlyingBuilder)
        {
            this.underlyingBuilder = underlyingBuilder;
        }

        #region Implementation of IValueBuilder

        public IValueBuilder CreateInstance()
        {
            return new RequiredValueBuilder(underlyingBuilder.CreateInstance());
        }

        public void UpdateValue(object o)
        {
            underlyingBuilder.UpdateValue(o);
        }

        public object BuildObject()
        {
            object result = underlyingBuilder.BuildObject();
            if (result == null)
            {
                throw new MissingValueException("A required field was not assigned.");
            }
            return result;
        }

        #endregion
    }
}
