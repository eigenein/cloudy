using System;

using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing.Reduction.Delegates
{
    public delegate void Reductor(IReducible reducible1, IReducible reducible2);

    public delegate void Reductor<TValue>(IReducible<TValue> reducible1, IReducible<TValue> reducible2);
}
