using StyletIoC.Creation;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StyletIoC
{
    /// <summary>
    /// Interface for selecting what to bind a service to.
    /// Call StyletIoCBuilder.Bind(..) to get an instance of this
    /// </summary>
    public interface IBindTo : IToAnyService, IWithKeyOrAndOrToMultipleServices, IWithKeyOrToMulipleServices
    {
    }

    public interface IToMultipleServices
    {
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
    }

    public interface IToAnyService : IToMultipleServices
    {
        /// <summary>
        /// Bind the specified service to itself - if you self-bind MyClass, and request an instance of MyClass, you'll get an instance of MyClass.
        /// </summary>
        /// <returns>Fluent interface to continue configuration</returns>
        IInScopeOrWithKeyOrAsWeakBinding ToSelf();

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

    public interface IAndTo
    {
        IWithKeyOrAndOrToMultipleServices And(Type serviceType);
        IWithKeyOrAndOrToMultipleServices And<TService>();
    }

    public interface IWithKeyOrToMulipleServices : IToMultipleServices
    {
        IAndOrToMultipleServices WithKey(string key);
    }

    public interface IAndOrToMultipleServices : IToMultipleServices, IAndTo
    {
    }

    public interface IWithKeyOrAndOrToMultipleServices : IWithKeyOrToMulipleServices, IAndTo
    {
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
}
