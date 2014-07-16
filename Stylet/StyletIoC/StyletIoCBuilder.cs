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
        IInScopeOrWithKey ToSelf();

        /// <summary>
        /// Bind the specified service to another type which implements that service. E.g. builder.Bind{IMyClass}().To{MyClass}(), and request an IMyClass: you'll get a MyClass.
        /// </summary>
        /// <typeparam name="TImplementation">Type to bind the service to</typeparam>
        IInScopeOrWithKey To<TImplementation>();

        /// <summary>
        /// Bind the specified service to another type which implements that service. E.g. builder.Bind{IMyClass}().To(typeof(MyClass)), and request an IMyClass: you'll get a MyClass.
        /// </summary>
        /// <param name="implementationType">Type to bind the service to</param>
        IInScopeOrWithKey To(Type implementationType);

        /// <summary>
        /// Bind the specified service to a factory delegate, which will be called when an instance is required. E.g. ...ToFactory(c => new MyClass(c.Get{Dependency}(), "foo"))
        /// </summary>
        /// <typeparam name="TImplementation">Type returned by the factory delegate. Must implement the service</typeparam>
        /// <param name="factory">Factory delegate to bind got</param>
        IInScopeOrWithKey ToFactory<TImplementation>(Func<IContainer, TImplementation> factory);

        /// <summary>
        /// If the service is an interface with a number of methods which return other types, generate an implementation of that abstract factory and bind it to the interface.
        /// </summary>
        IWithKey ToAbstractFactory();

        /// <summary>
        /// Discover all implementations of the service in the specified assemblies / the current assembly, and bind those to the service
        /// </summary>
        /// <param name="assemblies">Assemblies to search. If empty / null, searches the current assembly</param>
        IInScopeOrWithKey ToAllImplementations(params Assembly[] assemblies);
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
    /// Fluent interface on which InSingletonScope can be called
    /// </summary>
    public interface IInScope
    {
        /// <summary>
        /// Modify the scope of the binding to Singleton. One instance of this implementation will be generated for this binding.
        /// </summary>
        void InSingletonScope();
    }

    /// <summary>
    /// Fluent interface on which InSingletonScope or WithKey can be called
    /// </summary>
    public interface IInScopeOrWithKey : IInScope
    {
        /// <summary>
        /// Associate a key with this binding. Requests for the service will have to specify this key to retrieve the result of this binding
        /// </summary>
        /// <param name="key">Key to associate with this binding</param>
        IInScope WithKey(string key);
    }

    internal class BuilderBindTo : IBindTo
    {
        public Type ServiceType { get; private set; }
        private BuilderBindingBase builderBinding;
        public bool IsWeak { get; private set; }
        public string Key { get { return this.builderBinding.Key; } }

        public BuilderBindTo(Type serviceType, bool isWeak)
        {
            this.ServiceType = serviceType;
            this.IsWeak = isWeak;
        }

        public IInScopeOrWithKey ToSelf()
        {
            return this.To(this.ServiceType);
        }

        public IInScopeOrWithKey To<TImplementation>()
        {
            return this.To(typeof(TImplementation));
        }

        public IInScopeOrWithKey To(Type implementationType)
        {
            this.builderBinding = new BuilderTypeBinding(this.ServiceType, implementationType);
            return this.builderBinding;
        }

        public IInScopeOrWithKey ToFactory<TImplementation>(Func<IContainer, TImplementation> factory)
        {
            this.builderBinding = new BuilderFactoryBinding<TImplementation>(this.ServiceType, factory);
            return this.builderBinding;
        }

        public IWithKey ToAbstractFactory()
        {
            this.builderBinding = new AbstractFactoryBinding(this.ServiceType);
            return this.builderBinding;
        }

        public IInScopeOrWithKey ToAllImplementations(params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
                assemblies = new[] { Assembly.GetCallingAssembly() };
            this.builderBinding = new BuilderToAllImplementationsBinding(this.ServiceType, assemblies);
            return this.builderBinding;
        }

        internal void Build(StyletIoCContainer container)
        {
            this.builderBinding.Build(container);
        }
    }

    internal abstract class BuilderBindingBase : IInScopeOrWithKey, IWithKey
    {
        protected Type serviceType;
        protected Func<ICreator, IRegistration> registrationFactory;
        public string Key { get; protected set; }

        public BuilderBindingBase(Type serviceType)
        {
            this.serviceType = serviceType;

            // Default is transient
            this.registrationFactory = creator => new TransientRegistration(creator);
        }

        void IInScope.InSingletonScope()
        {
            this.registrationFactory = creator => new SingletonRegistration(creator);
        }

        IInScope IInScopeOrWithKey.WithKey(string key)
        {
            this.Key = key;
            return this;
        }

        protected void EnsureType(Type implementationType, Type serviceType = null)
        {
            serviceType = serviceType ?? this.serviceType;

            if (!implementationType.IsClass || implementationType.IsAbstract)
                throw new StyletIoCRegistrationException(String.Format("Type {0} is not a concrete class, and so can't be used to implemented service {1}", implementationType.Description(), serviceType.Description()));

            // Test this first, as it's a bit clearer than hitting 'type doesn't implement service'
            if (implementationType.IsGenericTypeDefinition)
            {
                if (!serviceType.IsGenericTypeDefinition)
                    throw new StyletIoCRegistrationException(String.Format("You can't use an unbound generic type to implement anything that isn't an unbound generic service. Service: {0}, Type: {1}", serviceType.Description(), implementationType.Description()));

                // This restriction may change when I figure out how to pass down the correct type argument
                if (serviceType.GetTypeInfo().GenericTypeParameters.Length != implementationType.GetTypeInfo().GenericTypeParameters.Length)
                    throw new StyletIoCRegistrationException(String.Format("If you're registering an unbound generic type to an unbound generic service, both service and type must have the same number of type parameters. Service: {0}, Type: {1}", serviceType.Description(), implementationType.Description()));
            }
            else if (serviceType.IsGenericTypeDefinition)
            {
                if (implementationType.GetGenericArguments().Length > 0)
                    throw new StyletIoCRegistrationException(String.Format("You cannot bind the bound generic type {0} to the unbound generic service {1}", implementationType.Description(), serviceType.Description()));
                else
                    throw new StyletIoCRegistrationException(String.Format("You cannot bind the non-generic type {0} to the unbound generic service {1}", implementationType.Description(), serviceType.Description()));
            }

            if (!implementationType.Implements(this.serviceType))
                throw new StyletIoCRegistrationException(String.Format("Type {0} does not implement service {1}", implementationType.Description(), serviceType.Description()));
        }

        // Convenience...
        protected void BindImplementationToService(StyletIoCContainer container, Type implementationType, Type serviceType = null)
        {
            serviceType = serviceType ?? this.serviceType;

            if (serviceType.IsGenericTypeDefinition)
            {
                var unboundGeneric = new UnboundGeneric(implementationType, container, this.registrationFactory);
                container.AddUnboundGeneric(new TypeKey(serviceType, this.Key), unboundGeneric);
            }
            else
            {
                var creator = new TypeCreator(implementationType, container);
                var registration = this.registrationFactory(creator);

                container.AddRegistration(new TypeKey(serviceType, this.Key ?? creator.AttributeKey), registration);
            }
        }

        void IWithKey.WithKey(string key)
        {
            this.Key = key;
        }

        public abstract void Build(StyletIoCContainer container);
    }

    internal class BuilderTypeBinding : BuilderBindingBase
    {
        private Type implementationType;

        public BuilderTypeBinding(Type serviceType, Type implementationType) : base(serviceType)
        {
            this.EnsureType(implementationType);
            this.implementationType = implementationType;
        }

        public override void Build(StyletIoCContainer container)
        {
            this.BindImplementationToService(container, this.implementationType);
        }
    }

    internal class BuilderFactoryBinding<TImplementation> : BuilderBindingBase
    {
        private Func<IContainer, TImplementation> factory;

        public BuilderFactoryBinding(Type serviceType, Func<IContainer, TImplementation> factory) : base(serviceType)
        {
            if (this.serviceType.IsGenericTypeDefinition)
                throw new StyletIoCRegistrationException(String.Format("A factory cannot be used to implement unbound generic type {0}", this.serviceType.Description()));
            this.EnsureType(typeof(TImplementation));
            this.factory = factory;
        }

        public override void Build(StyletIoCContainer container)
        {
            var creator = new FactoryCreator<TImplementation>(this.factory, container);
            var registration = this.registrationFactory(creator);

            container.AddRegistration(new TypeKey(this.serviceType, this.Key), registration);
        }
    }

    internal class BuilderToAllImplementationsBinding : BuilderBindingBase
    {
        private IEnumerable<Assembly> assemblies;

        public BuilderToAllImplementationsBinding(Type serviceType, IEnumerable<Assembly> assemblies) : base(serviceType)
        {
            this.assemblies = assemblies;
        }

        public override void Build(StyletIoCContainer container)
        {
            var candidates = from type in assemblies.Distinct().SelectMany(x => x.GetTypes())
                             let baseType = type.GetBaseTypesAndInterfaces().FirstOrDefault(x => x == this.serviceType || x.IsGenericType && x.GetGenericTypeDefinition() == this.serviceType)
                             where baseType != null
                             select new { Type = type, Base = baseType.ContainsGenericParameters ? baseType.GetGenericTypeDefinition() : baseType };

            foreach (var candidate in candidates)
            {
                try
                {
                    this.EnsureType(candidate.Type, candidate.Base);
                    this.BindImplementationToService(container, candidate.Type, candidate.Base);
                }
                catch (StyletIoCRegistrationException e)
                {
                    Debug.WriteLine(String.Format("Unable to auto-bind type {0} to {1}: {2}", candidate.Base.Name, candidate.Type.Description(), e.Message), "StyletIoC");
                }
            }
        }
    }

    internal class AbstractFactoryBinding : BuilderBindingBase
    {
        public AbstractFactoryBinding(Type serviceType) : base(serviceType)
        {
            if (serviceType.IsGenericTypeDefinition)
                throw new StyletIoCRegistrationException(String.Format("Unbound generic type {0} can't be used as an abstract factory", serviceType.Description()));
        }

        public override void Build(StyletIoCContainer container)
        {
            var factoryType = container.GetFactoryForType(this.serviceType);
            var creator = new TypeCreator(factoryType, container);
            var registration = new SingletonRegistration(creator);

            container.AddRegistration(new TypeKey(this.serviceType, this.Key), registration);
        }
    }

    /// <summary>
    /// This IStyletIoCBuilder is the only way to create an IContainer. Binding are registered using the builder, than an IContainer generated.
    /// </summary>
    public interface IStyletIoCBuilder
    {
        /// <summary>
        /// Bind the specified service (interface, abstract class, concrete class, unbound generic, etc) to something
        /// </summary>
        /// <typeparam name="TService">Service to bind</typeparam>
        IBindTo Bind<TService>();

        /// <summary>
        /// Bind the specified service (interface, abstract class, concrete class, unbound generic, etc) to something
        /// </summary>
        /// <param name="serviceType">Service to bind</param>
        IBindTo Bind(Type serviceType);

        /// <summary>
        /// Search the specified assembly(s) / the current assembly for concrete types, and self-bind them
        /// </summary>
        /// <param name="assemblies">Assembly(s) to search, or leave empty / null to search the current assembly</param>
        void Autobind(params Assembly[] assemblies);

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
        private List<BuilderBindTo> bindings = new List<BuilderBindTo>();

        /// <summary>
        /// Bind the specified service (interface, abstract class, concrete class, unbound generic, etc) to something
        /// </summary>
        /// <param name="serviceType">Service to bind</param>
        public IBindTo Bind(Type serviceType)
        {
            return this.BindInternal(serviceType, false);
        }

        internal IBindTo BindWeak(Type serviceType)
        {
            return this.BindInternal(serviceType, true);
        }

        internal IBindTo BindInternal(Type serviceType, bool isWeak)
        {
            var builderBindTo = new BuilderBindTo(serviceType, isWeak);
            this.bindings.Add(builderBindTo);
            return builderBindTo;
        }

        /// <summary>
        /// Bind the specified service (interface, abstract class, concrete class, unbound generic, etc) to something
        /// </summary>
        /// <typeparam name="TService">Service to bind</typeparam>
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
            // If they haven't given any assemblies, use the assembly of the caller
            if (assemblies == null || !assemblies.Any())
                assemblies = new[] { Assembly.GetCallingAssembly() };

            // We self-bind concrete classes only
            var classes = assemblies.Distinct().SelectMany(x => x.GetTypes()).Where(c => c.IsClass && !c.IsAbstract);
            foreach (var cls in classes)
            {
                // It's not actually possible for this to fail with a StyletIoCRegistrationException (at least currently)
                // It's a self-binding, and those are always safe (at this stage - it could fall over when the containing's actually build)
                
                this.BindWeak(cls).To(cls);
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
        /// Once all bindings have been set, build an IContainer from which instances can be fetches
        /// </summary>
        /// <returns>An IContainer, which should be used from now on</returns>
        public IContainer BuildContainer()
        {
            var container = new StyletIoCContainer();
            container.AddRegistration(new TypeKey(typeof(IContainer), null), new SingletonRegistration(new FactoryCreator<StyletIoCContainer>(c => container, container)));
            //container.AddRegistration(new TypeKey(typeof(StyletIoCContainer), null), new SingletonRegistration(new FactoryCreator<StyletIoCContainer>(c => container, container)));

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
}
