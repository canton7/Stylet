using StyletIoC.Internal.Builders;
using System;
using System.Collections.Generic;

namespace StyletIoC
{
    /// <summary>
    /// Module which contains its own bindings, and can be added to a builder
    /// </summary>
    public abstract class StyletIoCModule
    {
        private readonly List<BuilderBindTo> bindings = new List<BuilderBindTo>();

        /// <summary>
        /// Bind the specified service (interface, abstract class, concrete class, unbound generic, etc) to something
        /// </summary>
        /// <param name="serviceType">Service to bind</param>
        /// <returns>Fluent interface to continue configuration</returns>
        protected IBindTo Bind(Type serviceType)
        {
            var builderBindTo = new BuilderBindTo(serviceType);
            this.bindings.Add(builderBindTo);
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

        internal void AddToBuilder(StyletIoCBuilder builder)
        {
            this.bindings.Clear();

            this.Load();

            foreach (var binding in this.bindings)
            {
                builder.AddBinding(binding);
            }
        }
    }
}
