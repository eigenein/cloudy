using System;

namespace Cloudy.Protobuf.Interfaces
{
    public interface IValueBuilder
    {
        IValueBuilder CreateInstance();

        void UpdateValue(object o);

        object BuildObject();
    }
}
