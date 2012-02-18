using System;
using Cloudy.Computing.Interfaces;

namespace Cloudy.Computing.Reduction.ReducibleTypes
{
    public class ReducibleBoolean : Reducible<bool>
    {
        public ReducibleBoolean(bool value) : base(value)
        {
            // Do nothing.
        }

        public override void LogicalAnd(IReducible other)
        {
            value &= ((ReducibleBoolean)other).value;
        }

        public override void LogicalOr(IReducible other)
        {
            value |= ((ReducibleBoolean)other).value;
        }

        public override void LogicalXor(IReducible other)
        {
            value ^= ((ReducibleBoolean)other).value;
        }
    }
}
