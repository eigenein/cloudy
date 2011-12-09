using System;
using System.Collections.Generic;

namespace Cloudy.Computing.Interfaces
{
    /// <summary>
    /// Represents an environment of a computing thread.
    /// </summary>
    public interface IEnvironment
    {
        /// <summary>
        /// Gets the ID of the current thread.
        /// </summary>
        Guid ThreadId { get; }

        /// <summary>
        /// Resolves a shortcut identifier to the corresponding threads identifiers.
        /// </summary>
        ICollection<Guid> ResolveId(Guid id);
    }
}
