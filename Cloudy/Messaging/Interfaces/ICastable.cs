using System;

namespace Cloudy.Messaging.Interfaces
{
    /// <summary>
    /// Provides methods to cast the object to the specified type.
    /// </summary>
    public interface ICastable
    {
        T Cast<T>();

        object Cast(Type type);
    }
}
