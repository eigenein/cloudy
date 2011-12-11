using System;
using Cloudy.Computing.Structures;

namespace Cloudy.Helpers
{
    public delegate void ParameterizedEventHandler<T>(object sender, EventArgs<T> e);

    public delegate void ParameterizedEventHandler<T1, T2>(object sender, EventArgs<T1, T2> e);
}
