using System;

namespace Cloudy.Messaging.Interfaces
{
    /// <summary>
    /// Provides methods to cast the object to the specified type.
    /// </summary>
    public interface ICastableValue
    {
        T Get<T>();

        object Get(Type type);
    }
}
