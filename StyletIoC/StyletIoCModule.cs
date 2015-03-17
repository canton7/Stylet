using StyletIoC.Internal.Builders;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StyletIoC
{
    /// <summary>
    /// Module which contains its own bindings, and can be added to a builder
    /// </summary>
    public abstract class StyletIoCModule
    {
        private StyletIoCBuilder builder;
        private Func<IEnumerable<Assembly>, string, IEnumerable<Assembly>> getAssemblies;

        /// <summary>
        /// Bind the specified service (interface, abstract class, concrete class, unbound generic, etc) to something
        /// </summary>
        /// <param name="serviceType">Service to bind</param>
        /// <returns>Fluent interface to continue configuration</returns>
        protected IBindTo Bind(Type serviceType)
        {
            if (this.builder == null || this.getAssemblies == null)
                throw new InvalidOperationException("Bind should only be called from inside Load, and you must not call Load yourself");

            var builderBindTo = new BuilderBindTo(serviceType, this.getAssemblies);
            this.builder.AddBinding(builderBindTo);
            return builderBindTo;
        }

        /// <summary>
        /// Bind the specified service (interface, abstract class, concrete class, unbound generic, etc) to something
        /// </summary>
        /// <typeparam name="TService">Service to bind</typeparam>
        /// <returns>Fluent interface to continue configuration</returns>
        protected IBindTo Bind<TService>()
        {
            return this.Bind(typeof(TService));
        }

        /// <summary>
        /// Override, and use 'Bind' to add bindings to the module
        /// </summary>
        protected abstract void Load();

        internal void AddToBuilder(StyletIoCBuilder builder, Func<IEnumerable<Assembly>, string, IEnumerable<Assembly>> getAssemblies)
        {
            this.builder = builder;
            this.getAssemblies = getAssemblies;

            this.Load();

            this.builder = null;
            this.getAssemblies = null;
        }
    }
}
