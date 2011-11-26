using System;

namespace Cloudy.Helpers
{
    public delegate void ParametrizedEventHandler<T>(object sender, EventArgs<T> e);

    public delegate void ParametrizedEventHandler<T1, T2>(object sender, EventArgs<T1, T2> e);
}
