using System;

namespace Cloudy.Messaging.Interfaces
{
    public interface IValue
    {
        T Get<T>();

        object Get(Type type);
    }
}
