using System;

namespace Cloudy.Protobuf.Interfaces
{
    /// <summary>
    /// Describes a value builder that used to construct a value during 
    /// deserialization.
    /// </summary>
    public interface IValueBuilder
    {
        /// <summary>
        /// Creates a new instance of the builder.
        /// </summary>
        IValueBuilder CreateInstance();

        /// <summary>
        /// Called when the value is read from the input stream.
        /// </summary>
        void UpdateValue(object o);

        /// <summary>
        /// Called to get a final value to set into a deserialized object.
        /// </summary>
        object BuildObject();
    }
}
