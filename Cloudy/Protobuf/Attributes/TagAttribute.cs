using System;

namespace Cloudy.Protobuf.Attributes
{
    /// <summary>
    /// Marks a property for serialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class TagAttribute : Attribute
    {
        private readonly int tag;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="tag">The field number.</param>
        public TagAttribute(int tag)
        {
            this.tag = tag;
        }

        /// <summary>
        /// Gets the field number.
        /// </summary>
        public int Tag
        {
            get { return tag; }
        }
    }
}
