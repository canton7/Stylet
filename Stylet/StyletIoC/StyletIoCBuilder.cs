using StyletIoC.Creation;
using StyletIoC.Internal;
using StyletIoC.Internal.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StyletIoC
{
    /// <summary>
    /// This IStyletIoCBuilder is the only way to create an IContainer. Binding are registered using the builder, than an IContainer generated.
    /// </summary>
    public interface IStyletIoCBuilder
    {
        /// <summary>
        /// Gets or sets the list of assemblies searched by Autobind and ToAllImplementatinos
        /// </summary>
        List<Assembly> Assemblies { get; set; }

        /// <summary>
        /// Bind the specified service (interface, abstract class, concrete class, unbound generic, etc) to something
        /// </summary>
        /// <param name="serviceType">Service to bind</param>
        /// <returns>Fluent interface to continue configuration</returns>
        IBindTo Bind(Type serviceType);

        /// <summary>
        /// Bind the specified service (interface, abstract class, concrete class, unbound generic, etc) to something
        /// </summary>
        /// <typeparam name="TService">Service to bind</typeparam>
        /// <returns>Fluent interface to continue configuration</returns>
        IBindTo Bind<TService>();

        /// <summary>
        /// Search the specified assembly(s) / the current assembly for concrete types, and self-bind them
        /// </summary>
        /// <param name="assemblies">Assembly(s) to search, or leave empty / null to search the current assembly</param>
        void Autobind(IEnumerable<Assembly> assemblies);

        /// <summary>
        /// Search the specified assembly(s) / the current assembly for concrete types, and self-bind them
        /// </summary>
        /// <param name="assemblies">Assembly(s) to search, or leave empty / null to search the current assembly</param>
        void Autobind(params Assembly[] assemblies);

        /// <summary>
        /// Add a single module to this builder
        /// </summary>
        /// <param name="module">Module to add</param>
        void AddModule(StyletIoCModule module);

        /// <summary>
        /// Add many modules to this builder
        /// </summary>
        /// <param name="modules">Modules to add</param>
        void AddModules(params StyletIoCModule[] modules);

        /// <summary>
        /// Once all bindings have been set, build an IContainer from which instances can be fetched
        /// </summary>
        /// <returns>An IContainer, which should be used from now on</returns>
        IContainer BuildContainer();
    }

    /// <summary>
    /// This StyletIoCBuilder is the only way to create an IContainer. Binding are registered using the builder, than an IContainer generated.
    /// </summary>
    public class StyletIoCBuilder : IStyletIoCBuilder
    {
        private readonly List<BuilderBindTo> bindings = new List<BuilderBindTo>();
        private List<Assembly> autobindAssemblies;

        /// <summary>
        /// Gets or sets the list of assemblies searched by Autobind and ToAllImplementatinos
        /// </summary>
        public List<Assembly> Assemblies { get; set; }
        
        /// <summary>
        /// Initialises a new instance of the <see cref="StyletIoCBuilder"/> class
        /// </summary>
        public StyletIoCBuilder()
        {
            this.Assemblies = new List<Assembly>() { Assembly.GetCallingAssembly() };
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="StyletIoCBuilder"/> class, which contains the given modules
        /// </summary>
        /// <param name="modules">Modules to add to the builder</param>
        public StyletIoCBuilder(params StyletIoCModule[] modules) : this()
        {
            this.AddModules(modules);
        }

        /// <summary>
        /// Bind the specified service (interface, abstract class, concrete class, unbound generic, etc) to something
        /// </summary>
        /// <param name="serviceType">Service to bind</param>
        /// <returns>Fluent interface to continue configuration</returns>
        public IBindTo Bind(Type serviceType)
        {
            var builderBindTo = new BuilderBindTo(serviceType, this.GetAssemblies);
            this.bindings.Add(builderBindTo);
            return builderBindTo;
        }

        /// <summary>
        /// Bind the specified service (interface, abstract class, concrete class, unbound generic, etc) to something
        /// </summary>
        /// <typeparam name="TService">Service to bind</typeparam>
        /// <returns>Fluent interface to continue configuration</returns>
        public IBindTo Bind<TService>()
        {
            return this.Bind(typeof(TService));
        }

        /// <summary>
        /// Search assemblies for concrete types, and self-bind them
        /// </summary>
        /// <param name="assemblies">Assemblies to search, in addition to the Assemblies property</param>
        public void Autobind(IEnumerable<Assembly> assemblies)
        {
            // If they've called Autobind before, then add the new set of assemblies on
            var existing = this.autobindAssemblies ?? Enumerable.Empty<Assembly>();
            this.autobindAssemblies = existing.Concat(this.GetAssemblies(assemblies, "Autobind")).Distinct().ToList();
        }

        /// <summary>
        /// Search assemblies for concrete types, and self-bind them
        /// </summary>
        /// <param name="assemblies">Assemblies to search, in addition to the Assemblies property</param>
        public void Autobind(params Assembly[] assemblies)
        {
            this.Autobind(assemblies.AsEnumerable());
        }

        /// <summary>
        /// Add a single module to this builder
        /// </summary>
        /// <param name="module">Module to add</param>
        public void AddModule(StyletIoCModule module)
        {
            module.AddToBuilder(this, this.GetAssemblies);
        }

        /// <summary>
        /// Add many modules to this builder
        /// </summary>
        /// <param name="modules">Modules to add</param>
        public void AddModules(params StyletIoCModule[] modules)
        {
            foreach (var module in modules)
            {
                this.AddModule(module);
            }
        }

        /// <summary>
        /// Once all bindings have been set, build an IContainer from which instances can be fetched
        /// </summary>
        /// <returns>An IContainer, which should be used from now on</returns>
        public IContainer BuildContainer()
        {
            var container = new Container(this.autobindAssemblies);

            // Just in case they want it
            this.Bind<IContainer>().ToInstance(container).DisposeWithContainer(false).AsWeakBinding();

            // For each binding which is weak, if another binding exists with any of the same type+key which is strong, we remove this binding
            var groups = (from binding in this.bindings
                          from serviceType in binding.ServiceTypes
                          select new { ServiceType = serviceType, Binding = binding })
                          .ToLookup(x => x.ServiceType);

            var filtered = from binding in this.bindings
                           where !(binding.IsWeak &&
                                binding.ServiceTypes.Any(serviceType => groups.Contains(serviceType) && groups[serviceType].Any(groupItem => !groupItem.Binding.IsWeak)))
                           select binding;

            foreach (var binding in filtered)
            {
                binding.Build(container);
            }
            return container;
        }

        internal void AddBinding(BuilderBindTo binding)
        {
            this.bindings.Add(binding);
        }

        private IEnumerable<Assembly> GetAssemblies(IEnumerable<Assembly> extras, string methodName)
        {
            IEnumerable<Assembly> assemblies = this.Assemblies ?? Enumerable.Empty<Assembly>();
            if (extras != null)
                assemblies = assemblies.Concat(extras);
            if (!assemblies.Any())
                throw new StyletIoCRegistrationException(String.Format("{0} called but Assemblies is empty, and no extra assemblies given", methodName));
            return assemblies.Distinct();
        }
    }
}
