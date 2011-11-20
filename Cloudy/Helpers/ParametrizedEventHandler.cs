using System;

namespace Cloudy.Helpers
{
    public delegate void ParametrizedEventHandler<T>(object sender, EventArgs<T> e);
}
