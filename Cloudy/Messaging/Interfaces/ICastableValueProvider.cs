using System;

namespace Cloudy.Messaging.Interfaces
{
    /// <summary>
    /// Provides methods to cast the object to the specified type.
    /// </summary>
    public interface ICastableValueProvider
    {
        T GetValue<T>();

        object GetValue(Type type);
    }
}
