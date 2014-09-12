using StyletIoC.Creation;
using StyletIoC.Internal;
using StyletIoC.Internal.Builders;
using StyletIoC.Internal.Creators;
using StyletIoC.Internal.Registrations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// <returns></returns>
        IInScopeOrWithKeyOrAsWeakBinding ToSelf();

        /// <summary>
        /// Bind the specified service to another type which implements that service. E.g. builder.Bind{IMyClass}().To(typeof(MyClass)), and request an IMyClass: you'll get a MyClass.
        /// </summary>
        /// <param name="implementationType">Type to bind the service to</param>
        IInScopeOrWithKeyOrAsWeakBinding To(Type implementationType);

        /// <summary>
        /// Bind the specified service to a factory delegate, which will be called when an instance is required. E.g. ...ToFactory(c => new MyClass(c.Get{Dependency}(), "foo"))
        /// </summary>
        /// <typeparam name="TImplementation">Type returned by the factory delegate. Must implement the service</typeparam>
        /// <param name="factory">Factory delegate to bind got</param>
        IInScopeOrWithKeyOrAsWeakBinding ToFactory<TImplementation>(Func<IRegistrationContext, TImplementation> factory);

        /// <summary>
        /// If the service is an interface with a number of methods which return other types, generate an implementation of that abstract factory and bind it to the interface.
        /// </summary>
        IWithKey ToAbstractFactory();

        /// <summary>
        /// Discover all implementations of the service in the specified assemblies / the current assembly, and bind those to the service
        /// </summary>
        /// <param name="assemblies">Assemblies to search. If empty / null, searches the current assembly</param>
        IInScopeOrWithKeyOrAsWeakBinding ToAllImplementations(IEnumerable<Assembly> assemblies);
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
        /// When the container is built, each collection of registrations for each Type+key combination is examined.
        /// If only weak bindings exist, then all bindings are built into the container.
        /// If any normal bindings exist, then all weak bindings are ignored, and only the normal bindings are built into the container.
        /// 
        /// This is very useful for integration StyletIoC into a framework. The framework can add default bindings for services as
        /// weak bindings, and the user can use normal bindings. If the user does specify a binding, then this will override
        /// the binding set by the framework.
        /// 
        /// This is also used by AutoBind when self-binding concrete types, for the sme reason.
        /// </remarks>
        void AsWeakBinding();
    }

    /// <summary>
    /// Fluent interface on which WithKey can be called
    /// </summary>
    public interface IWithKey
    {
        /// <summary>
        /// Associate a key with this binding. Requests for the service will have to specify this key to retrieve the result of this binding
        /// </summary>
        /// <param name="key">Key to associate with this binding</param>
        void WithKey(string key);
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
        IAsWeakBinding WithRegistrationFactory(RegistrationFactory registrationFactory);
    }

    /// <summary>
    /// Fluent interface on which WithKey, or the scoping extensions can be called
    /// </summary>
    public interface IInScopeOrWithKeyOrAsWeakBinding : IInScopeOrAsWeakBinding
    {
        /// <summary>
        /// Associate a key with this binding. Requests for the service will have to specify this key to retrieve the result of this binding
        /// </summary>
        /// <param name="key">Key to associate with this binding</param>
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
        IBindTo Bind(Type serviceType);

        /// <summary>
        /// Search the specified assembly(s) / the current assembly for concrete types, and self-bind them
        /// </summary>
        /// <param name="assemblies">Assembly(s) to search, or leave empty / null to search the current assembly</param>
        void Autobind(IEnumerable<Assembly> assemblies);

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
        private readonly Container parent;
        private List<BuilderBindTo> bindings = new List<BuilderBindTo>();

        /// <summary>
        /// Create a new StyletIoC buidler instance
        /// </summary>
        public StyletIoCBuilder() : this(null) { }

        internal StyletIoCBuilder(Container parent)
        {
            this.parent = parent;
        }

        /// <summary>
        /// Bind the specified service (interface, abstract class, concrete class, unbound generic, etc) to something
        /// </summary>
        /// <param name="serviceType">Service to bind</param>
        public IBindTo Bind(Type serviceType)
        {
            var builderBindTo = new BuilderBindTo(serviceType);
            this.bindings.Add(builderBindTo);
            return builderBindTo;
        }

        /// <summary>
        /// Search the specified assembly(s) / the current assembly for concrete types, and self-bind them
        /// </summary>
        /// <param name="assemblies">Assembly(s) to search, or leave empty / null to search the current assembly</param>
        public void Autobind(IEnumerable<Assembly> assemblies)
        {
            // If they haven't given any assemblies, use the assembly of the caller
            if (assemblies == null || !assemblies.Any())
                assemblies = new[] { Assembly.GetCallingAssembly() };

            // We self-bind concrete classes only
            var classes = assemblies.Distinct().SelectMany(x => x.GetTypes()).Where(c => c.IsClass && !c.IsAbstract);
            foreach (var cls in classes)
            {
                // It's not actually possible for this to fail with a StyletIoCRegistrationException (at least currently)
                // It's a self-binding, and those are always safe (at this stage - it could fall over when the containing's actually build)
                this.Bind(cls).To(cls).AsWeakBinding();
            }
        }

        /// <summary>
        /// Once all bindings have been set, build an IContainer from which instances can be fetches
        /// </summary>
        /// <returns>An IContainer, which should be used from now on</returns>
        public IContainer BuildContainer()
        {
            var container = this.parent == null ? new Container() : new Container(this.parent);

            // For each TypeKey, we remove any weak bindings if there are any strong bindings
            var groups = this.bindings.GroupBy(x =>  new { Key = x.Key, Type = x.ServiceType });
            var filtered = groups.SelectMany(group => group.Any(x => !x.IsWeak) ? group.Where(x => !x.IsWeak) : group);
            foreach (var binding in filtered)
            {
                binding.Build(container);
            }
            return container;
        }
    }

    /// <summary>
    /// Extra methods on IStyletIoCBuilder which are useful
    /// </summary>
    public static class StyletIoCBuilderExtensions
    {
        /// <summary>
        /// Search the specified assembly(s) / the current assembly for concrete types, and self-bind them
        /// </summary>
        /// <param name="builder">Builder to call on</param>
        /// <param name="assemblies">Assembly(s) to search, or leave empty / null to search the current assembly</param>
        public static void Autobind(this IStyletIoCBuilder builder, params Assembly[] assemblies)
        {
            // Have to do null-or-empty check here as well, otherwise GetCallingAssembly returns this one....
            if (assemblies == null || assemblies.Length == 0)
                assemblies = new[] { Assembly.GetCallingAssembly() };
            builder.Autobind(assemblies.AsEnumerable());
        }

        /// <summary>
        /// Bind the specified service (interface, abstract class, concrete class, unbound generic, etc) to something
        /// </summary>
        /// <typeparam name="TService">Service to bind</typeparam>
        public static IBindTo Bind<TService>(this IStyletIoCBuilder builder)
        {
            return builder.Bind(typeof(TService));
        }

        /// <summary>
        /// Bind the specified service to another type which implements that service. E.g. builder.Bind{IMyClass}().To{MyClass}(), and request an IMyClass: you'll get a MyClass.
        /// </summary>
        /// <typeparam name="TImplementation">Type to bind the service to</typeparam>
        public static IInScopeOrWithKeyOrAsWeakBinding To<TImplementation>(this IBindTo bindTo)
        {
            return bindTo.To(typeof(TImplementation));
        }

        /// <summary>
        /// Discover all implementations of the service in the specified assemblies / the current assembly, and bind those to the service
        /// </summary>
        /// <param name="bindTo">Binder to call on</param>
        /// <param name="assemblies">Assemblies to search. If empty / null, searches the current assembly</param>
        public static IInScopeOrWithKeyOrAsWeakBinding ToAllImplementations(this IBindTo bindTo, params Assembly[] assemblies)
        {
            // Have to do null-or-empty check here as well, otherwise GetCallingAssembly returns this one....
            if (assemblies == null || assemblies.Length == 0)
                assemblies = new[] { Assembly.GetCallingAssembly() };
            return bindTo.ToAllImplementations(assemblies.AsEnumerable());
        }

        /// <summary>
        /// Modify the scope of the binding to Singleton. One instance of this implementation will be generated for this binding.
        /// </summary>
        public static IAsWeakBinding InSingletonScope(this IInScopeOrAsWeakBinding builder)
        {
            return builder.WithRegistrationFactory((ctx, creator, key) => new SingletonRegistration(ctx, creator));
        }

        /// <summary>
        /// Modify the scope binding to Per Container. One instance of this implementation will be generated per container / child container.
        /// </summary>
        public static IAsWeakBinding InPerContainerScope(this IInScopeOrAsWeakBinding builder)
        {
            return builder.WithRegistrationFactory((ctx, creator, key) => new PerContainerRegistration(ctx, creator, key));
        }
    }
}
