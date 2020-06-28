﻿using System;

namespace Unity.Resolution
{
    /// <summary>
    /// Base class for all override objects passed in the
    /// <see cref="IUnityContainer.Resolve"/> method.
    /// </summary>
    public abstract class ResolverOverride : IResolve
    {
        #region Fields

        protected Type?            Target;
        protected readonly string? Name;
        protected readonly object? Value;

        #endregion


        #region Constructors

        /// <summary>
        /// This constructor is used when no target is required
        /// </summary>
        /// <param name="name">Name of the dependency</param>
        /// <param name="value">Value to pass to resolver</param>
        protected ResolverOverride(string? name, object? value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// This constructor is used with targeted overrides
        /// </summary>
        /// <param name="target"><see cref="Type"/> of the target</param>
        /// <param name="name">Name of the dependency</param>
        /// <param name="value">Value to pass to resolver</param>
        protected ResolverOverride(Type? target, string? name, object? value)
        {
            Target = target;
            Name = name;
            Value = value;
        }

        #endregion


        #region Type Based Override

        /// <summary>
        /// This method adds target information to the override. Only targeted
        /// <see cref="Type"/> will be overridden even if other dependencies match
        /// the type of the name of the override.
        /// </summary>
        /// <typeparam name="T">Type to constrain the override to.</typeparam>
        /// <returns>The new override.</returns>
        public ResolverOverride OnType<T>()
        {
            Target = typeof(T);
            return this;
        }

        /// <summary>
        /// This method adds target information to the override. Only targeted
        /// <see cref="Type"/> will be overridden even if other dependencies match
        /// the type of the name of the override.
        /// </summary>
        /// <param name="targetType">Type to constrain the override to.</param>
        /// <returns>The new override.</returns>
        public ResolverOverride OnType(Type targetType)
        {
            Target = targetType;
            return this;
        }

        #endregion


        #region IResolve

        public virtual object? Resolve<TContext>(ref TContext context)
            where TContext : IResolveContext
        {
            return Value switch
            {
                IResolve resolve               => resolve.Resolve(ref context),
                IResolverFactory<Type> factory => factory.GetResolver<TContext>(context.Type)
                                                         .Invoke(ref context),
                _ => Value,
            };
        }

        public virtual ResolveDelegate<TContext> GetResolver<TContext>(Type type)
            where TContext : IResolveContext
        {
            return Value switch
            {
                IResolve resolve               => resolve.Resolve,
                IResolverFactory<Type> factory => factory.GetResolver<TContext>(type),
                _ => (ref TContext context)    => Value,
            };
        }

        #endregion


        #region Object

        public override int GetHashCode()
        {
            return ((Target?.GetHashCode() ?? 0 * 37) + (Name?.GetHashCode() ?? 0 * 17)) ^ GetType().GetHashCode();

        }

        public override bool Equals(object? obj)
        {
            return this == obj as ResolverOverride;
        }

        public static bool operator ==(ResolverOverride? left, ResolverOverride? right)
        {
            return left?.GetHashCode() == right?.GetHashCode();
        }

        public static bool operator !=(ResolverOverride? left, ResolverOverride? right)
        {
            return !(left == right);
        }

        #endregion
    }
}