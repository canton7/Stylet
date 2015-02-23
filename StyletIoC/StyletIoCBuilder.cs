using StyletIoC.Creation;
using StyletIoC.Internal;
using StyletIoC.Internal.Builders;
using StyletIoC.Internal.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StyletIoC
{
    /// <summary>
    /// Interface for selecting what to bind a service to.
    /// Call StyletIoCBuilder.Bind(..) to get an instance of this
    /// </summary>
    public interface IBindTo
    {
        /// <summary>
        /// Bind the specified service to itself - if you self-bind MyClass, and request an instance of MyClass, you'll get an instance of MyClass.
        /// </summary>
        /// <returns>Fluent interface to continue configuration</returns>
        IInScopeOrWithKeyOrAsWeakBinding ToSelf();

        /// <summary>
        /// Bind the specified service to another type which implements that service. E.g. builder.Bind{IMyClass}().To(typeof(MyClass)), and request an IMyClass: you'll get a MyClass.
        /// </summary>
        /// <param name="implementationType">Type to bind the service to</param>
        /// <returns>Fluent interface to continue configuration</returns>
        IInScopeOrWithKeyOrAsWeakBinding To(Type implementationType);

        /// <summary>
        /// Bind the specified service to another type which implements that service. E.g. builder.Bind{IMyClass}().To{MyClass}(), and request an IMyClass: you'll get a MyClass.
        /// </summary>
        /// <typeparam name="TImplementation">Type to bind the service to</typeparam>
        /// <returns>Fluent interface to continue configuration</returns>
        IInScopeOrWithKeyOrAsWeakBinding To<TImplementation>();

        /// <summary>
        /// Bind the specified service to a factory delegate, which will be called when an instance is required. E.g. ...ToFactory(c => new MyClass(c.Get{Dependency}(), "foo"))
        /// </summary>
        /// <typeparam name="TImplementation">Type returned by the factory delegate. Must implement the service</typeparam>
        /// <param name="factory">Factory delegate to bind got</param>
        /// <returns>Fluent interface to continue configuration</returns>
        IInScopeOrWithKeyOrAsWeakBinding ToFactory<TImplementation>(Func<IRegistrationContext, TImplementation> factory);

        /// <summary>
        /// Bind the specified service to the given untyped instance
        /// </summary>
        /// <param name="instance">Instance to use</param>
        /// <returns>Fluent interface to continue configuration</returns>
        IWithKeyOrAsWeakBinding ToInstance(object instance);

        /// <summary>
        /// If the service is an interface with a number of methods which return other types, generate an implementation of that abstract factory and bind it to the interface.
        /// </summary>
        /// <returns>Fluent interface to continue configuration</returns>
        IWithKeyOrAsWeakBinding ToAbstractFactory();

        /// <summary>
        /// Discover all implementations of the service in the specified assemblies / the current assembly, and bind those to the service
        /// </summary>
        /// <param name="assemblies">Assemblies to search. If empty / null, searches the current assembly</param>
        /// <returns>Fluent interface to continue configuration</returns>
        IInScopeOrWithKeyOrAsWeakBinding ToAllImplementations(IEnumerable<Assembly> assemblies);

        /// <summary>
        /// Discover all implementations of the service in the specified assemblies / the current assembly, and bind those to the service
        /// </summary>
        /// <param name="assemblies">Assemblies to search. If empty / null, searches the current assembly</param>
        /// <returns>Fluent interface to continue configuration</returns>
        IInScopeOrWithKeyOrAsWeakBinding ToAllImplementations(params Assembly[] assemblies);
    }

    /// <summary>
    /// Fluent interface on which AsWeakBinding can be called
    /// </summary>
    public interface IAsWeakBinding
    {
        /// <summary>
        /// Mark the binding as weak
        /// </summary>
        /// <remarks>
        /// <para>
        /// When the container is built, each collection of registrations for each Type+key combination is examined.
        /// If only weak bindings exist, then all bindings are built into the container.
        /// If any normal bindings exist, then all weak bindings are ignored, and only the normal bindings are built into the container.
        /// </para>
        /// <para>
        /// This is very useful for integration StyletIoC into a framework. The framework can add default bindings for services as
        /// weak bindings, and the user can use normal bindings. If the user does specify a binding, then this will override
        /// the binding set by the framework.
        /// </para>
        /// <para>
        /// This is also used by AutoBind when self-binding concrete types, for the sme reason.
        /// </para>
        /// </remarks>
        void AsWeakBinding();
    }

    /// <summary>
    /// Fluent interface on which WithKey or AsWeakBinding can be called
    /// </summary>
    public interface IWithKeyOrAsWeakBinding : IAsWeakBinding
    {
        /// <summary>
        /// Associate a key with this binding. Requests for the service will have to specify this key to retrieve the result of this binding
        /// </summary>
        /// <param name="key">Key to associate with this binding</param>
        /// <returns>Fluent interface to continue configuration</returns>
        IAsWeakBinding WithKey(string key);
    }

    /// <summary>
    /// Fluent interface on which methods to modify the scope can be called
    /// </summary>
    public interface IInScopeOrAsWeakBinding : IAsWeakBinding
    {
        /// <summary>
        /// Specify a factory that creates an IRegistration to use for this binding
        /// </summary>
        /// <param name="registrationFactory">Registration factory to use</param>
        /// <returns>Fluent interface to continue configuration</returns>
        IAsWeakBinding WithRegistrationFactory(RegistrationFactory registrationFactory);

        /// <summary>
        /// Modify the scope of the binding to Singleton. One instance of this implementation will be generated for this binding.
        /// </summary>
        /// <returns>Fluent interface to continue configuration</returns>
        IAsWeakBinding InSingletonScope();
    }

    /// <summary>
    /// Fluent interface on which WithKey, AsWeakBinding, or the scoping extensions can be called
    /// </summary>
    public interface IInScopeOrWithKeyOrAsWeakBinding : IInScopeOrAsWeakBinding
    {
        /// <summary>
        /// Associate a key with this binding. Requests for the service will have to specify this key to retrieve the result of this binding
        /// </summary>
        /// <param name="key">Key to associate with this binding</param>
        /// <returns>Fluent interface to continue configuration</returns>
        IInScopeOrAsWeakBinding WithKey(string key);
    }

    /// <summary>
    /// This IStyletIoCBuilder is the only way to create an IContainer. Binding are registered using the builder, than an IContainer generated.
    /// </summary>
    public interface IStyletIoCBuilder
    {
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
        /// Once all bindings have been set, build an IContainer from which instances can be fetches
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
        private readonly List<StyletIoCModule> modules = new List<StyletIoCModule>();

        /// <summary>
        /// Initialises a new instance of the <see cref="StyletIoCBuilder"/> class
        /// </summary>
        public StyletIoCBuilder() { }

        /// <summary>
        /// Initialises a new instance of the <see cref="StyletIoCBuilder"/> class, which contains the given modules
        /// </summary>
        /// <param name="modules">Modules to add to the builder</param>
        public StyletIoCBuilder(params StyletIoCModule[] modules)
        {
            this.modules.AddRange(modules);
        }

        /// <summary>
        /// Bind the specified service (interface, abstract class, concrete class, unbound generic, etc) to something
        /// </summary>
        /// <param name="serviceType">Service to bind</param>
        /// <returns>Fluent interface to continue configuration</returns>
        public IBindTo Bind(Type serviceType)
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
        public IBindTo Bind<TService>()
        {
            return this.Bind(typeof(TService));
        }

        /// <summary>
        /// Search the specified assembly(s) / the current assembly for concrete types, and self-bind them
        /// </summary>
        /// <param name="assemblies">Assembly(s) to search, or leave empty / null to search the current assembly</param>
        public void Autobind(IEnumerable<Assembly> assemblies)
        {
            var assembliesArray = (assemblies == null) ? new Assembly[0] : (assemblies as Assembly[] ?? assemblies.ToArray());
            // If they haven't given any assemblies, use the assembly of the caller
            if (assembliesArray.Length == 0)
                assembliesArray = new[] { Assembly.GetCallingAssembly() };

            // We self-bind concrete classes only
            var classes = assembliesArray.Distinct().SelectMany(x => x.GetTypes()).Where(c => c.IsClass && !c.IsAbstract);
            foreach (var cls in classes)
            {
                // It's not actually possible for this to fail with a StyletIoCRegistrationException (at least currently)
                // It's a self-binding, and those are always safe (at this stage - it could fall over when the containing's actually build)
                this.Bind(cls).To(cls).AsWeakBinding();
            }
        }

        /// <summary>
        /// Search the specified assembly(s) / the current assembly for concrete types, and self-bind them
        /// </summary>
        /// <param name="assemblies">Assembly(s) to search, or leave empty / null to search the current assembly</param>
        public void Autobind(params Assembly[] assemblies)
        {
            // Have to do null-or-empty check here as well, otherwise GetCallingAssembly returns this one....
            if (assemblies == null || assemblies.Length == 0)
                assemblies = new[] { Assembly.GetCallingAssembly() };
            this.Autobind(assemblies.AsEnumerable());
        }

        /// <summary>
        /// Add a single module to this builder
        /// </summary>
        /// <param name="module">Module to add</param>
        public void AddModule(StyletIoCModule module)
        {
            this.modules.Add(module);
        }

        /// <summary>
        /// Add many modules to this builder
        /// </summary>
        /// <param name="modules">Modules to add</param>
        public void AddModules(params StyletIoCModule[] modules)
        {
            this.modules.AddRange(modules);
        }

        /// <summary>
        /// Once all bindings have been set, build an IContainer from which instances can be fetches
        /// </summary>
        /// <returns>An IContainer, which should be used from now on</returns>
        public IContainer BuildContainer()
        {
            foreach (var module in this.modules)
            {
                module.AddToBuilder(this);
            }

            var container = new Container();

            // Just in case they want it
            this.Bind<IContainer>().ToInstance(container).AsWeakBinding();

            // For each TypeKey, we remove any weak bindings if there are any strong bindings
            var groups = this.bindings.GroupBy(x => new { Key = x.Key, Type = x.ServiceType });
            var filtered = groups.SelectMany(group => group.Any(x => !x.IsWeak) ? group.Where(x => !x.IsWeak) : group);
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
    }
}
