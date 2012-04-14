using System;
using System.Collections.Generic;
using Cloudy.Computing.Interfaces;
using Cloudy.Computing.Reduction.ReducibleTypes;

namespace Cloudy.Computing.Reduction
{
    public abstract class Reducible : IReducible
    {
        private static readonly IDictionary<Type, Func<object, IReducible>> Cache =
            new Dictionary<Type, Func<object, IReducible>>();

        static Reducible()
        {
            Cache.Add(typeof(String), value => new ReducibleString((string)value));
            Cache.Add(typeof(Byte), value => new ReducibleByte((byte)value));
            Cache.Add(typeof(SByte), value => new ReducibleSByte((sbyte)value));
            Cache.Add(typeof(Int32), value => new ReducibleInt32((int)value));
            Cache.Add(typeof(UInt32), value => new ReducibleUInt32((uint)value));
            Cache.Add(typeof(Int16), value => new ReducibleInt16((short)value));
            Cache.Add(typeof(UInt16), value => new ReducibleUInt16((ushort)value));
            Cache.Add(typeof(Int64), value => new ReducibleInt64((long)value));
            Cache.Add(typeof(UInt64), value => new ReducibleUInt64((ulong)value));
            Cache.Add(typeof(Single), value => new ReducibleSingle((float)value));
            Cache.Add(typeof(Double), value => new ReducibleDouble((double)value));
            Cache.Add(typeof(Boolean), value => new ReducibleBoolean((bool)value));
            Cache.Add(typeof(Decimal), value => new ReducibleDecimal((decimal)value));
            Cache.Add(typeof(Char), value => new ReducibleChar((char)value));
        }

        /// <summary>
        /// Wraps the value that implements <see cref="IReducible"/>.
        /// </summary>
        public static IReducible<TValue> Create<TValue>(TValue value)
        {
            Func<object, IReducible> constructor;
            if (!Cache.TryGetValue(typeof(TValue), out constructor))
            {
                throw new NotSupportedException(String.Format(
                    "The type is not supported: {0}.", typeof(TValue)));
            }
            return (IReducible<TValue>)constructor(value);
        }

        /// <summary>
        /// Adds the custom reducible type.
        /// </summary>
        public static void AddCustomType(Type type, Func<object, IReducible> constructor)
        {
            Cache.Add(type, constructor);
        }

        #region Implementation of IReducible

        public virtual void Add(IReducible other)
        {
            throw new NotImplementedException("The operation should be overridden.");
        }

        public virtual void Multiply(IReducible other)
        {
            throw new NotImplementedException("The operation should be overridden.");
        }

        public virtual void LogicalAnd(IReducible other)
        {
            throw new NotImplementedException("The operation should be overridden.");
        }

        public virtual void BitwiseAnd(IReducible other)
        {
            throw new NotImplementedException("The operation should be overridden.");
        }

        public virtual void LogicalOr(IReducible other)
        {
            throw new NotImplementedException("The operation should be overridden.");
        }

        public virtual void BitwiseOr(IReducible other)
        {
            throw new NotImplementedException("The operation should be overridden.");
        }

        public virtual void LogicalXor(IReducible other)
        {
            throw new NotImplementedException();
        }

        public virtual void BitwiseXor(IReducible other)
        {
            throw new NotImplementedException("The operation should be overridden.");
        }

        public virtual void Maximize(IReducible other)
        {
            throw new NotImplementedException("The operation should be overridden.");
        }

        public virtual void Minimize(IReducible other)
        {
            throw new NotImplementedException("The operation should be overridden.");
        }

        #endregion
    }

    public abstract class Reducible<TValue> : Reducible, IReducible<TValue>
    {
        protected TValue value;

        protected Reducible(TValue value)
        {
            this.value = value;
        }

        #region Implementation of IReducible<TValue>

        public TValue Value
        {
            get { return value; }
        }

        #endregion
    }
}
