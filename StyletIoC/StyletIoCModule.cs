using StyletIoC.Internal.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
