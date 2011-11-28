using System;

namespace Cloudy.Messaging.Interfaces
{
    /// <summary>
    /// Represents a message with an attached value.
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// Gets the tag of the message.
        /// </summary>
        int Tag { get; }

        /// <summary>
        /// Gets the value attached.
        /// </summary>
        T Get<T>();

        /// <summary>
        /// Gets the value attached.
        /// </summary>
        object Get(Type type);
    }
}
