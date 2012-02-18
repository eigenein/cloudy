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
            Cache.Add(typeof(String), value => new StringReducible((string)value));
            Cache.Add(typeof(Byte), value => new ByteReducible((byte)value));
        }

        public static IReducible Create<TValue>(TValue value)
        {
            Func<object, IReducible> constructor;
            if (!Cache.TryGetValue(typeof(TValue), out constructor))
            {
                throw new NotSupportedException(String.Format(
                    "The type is not supported: {0}.", typeof(TValue)));
            }
            return constructor(value);
        }

        public static void AddCustom(Type type, Func<object, IReducible> constructor)
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
