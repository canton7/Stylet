using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StyletIoC
{
    public interface IInScopeOrWithKey : IInScope
    {
        IInScope WithKey(string key);
    }
    public interface IWithKey
    {
        void WithKey(string key);
    }
    public interface IInScope
    {
        void InSingletonScope();
    }

    public interface IBindTo
    {
        IInScopeOrWithKey ToSelf();
        IInScopeOrWithKey To<TImplementation>();
        IInScopeOrWithKey To(Type implementationType);
        IInScopeOrWithKey ToFactory<TImplementation>(Func<IContainer, TImplementation> factory);
        IWithKey ToAbstractFactory();
        IInScopeOrWithKey ToAllImplementations(params Assembly[] assemblies);
    }
    internal class BuilderBindTo : IBindTo
    {
        private Type serviceType;
        private BuilderBindingBase builderBinding;

        public BuilderBindTo(Type serviceType)
        {
            this.serviceType = serviceType;
        }

        public IInScopeOrWithKey ToSelf()
        {
            return this.To(this.serviceType);
        }

        public IInScopeOrWithKey To<TImplementation>()
        {
            return this.To(typeof(TImplementation));
        }

        public IInScopeOrWithKey To(Type implementationType)
        {
            this.builderBinding = new BuilderTypeBinding(this.serviceType, implementationType);
            return this.builderBinding;
        }

        public IInScopeOrWithKey ToFactory<TImplementation>(Func<IContainer, TImplementation> factory)
        {
            this.builderBinding = new BuilderFactoryBinding<TImplementation>(this.serviceType, factory);
            return this.builderBinding;
        }

        public IWithKey ToAbstractFactory()
        {
            this.builderBinding = new AbstractFactoryBinding(this.serviceType);
            return this.builderBinding;
        }

        public IInScopeOrWithKey ToAllImplementations(params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
                assemblies = new[] { Assembly.GetCallingAssembly() };
            this.builderBinding = new BuilderToAllImplementationsBinding(this.serviceType, assemblies);
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
        protected bool isSingleton;
        protected string key;

        public BuilderBindingBase(Type serviceType)
        {
            this.serviceType = serviceType;
        }

        void IInScope.InSingletonScope()
        {
            this.isSingleton = true;
        }

        IInScope IInScopeOrWithKey.WithKey(string key)
        {
            this.key = key;
            return this;
        }

        protected void EnsureType(Type implementationType)
        {
            if (!implementationType.IsClass || implementationType.IsAbstract)
                throw new StyletIoCRegistrationException(String.Format("Type {0} is not a concrete class, and so can't be used to implemented service {1}", implementationType.Name, this.serviceType.Name));

            // Test this first, as it's a bit clearer than hitting 'type doesn't implement service'
            if (implementationType.IsGenericTypeDefinition)
            {
                if (this.isSingleton)
                    throw new StyletIoCRegistrationException(String.Format("You cannot create singleton registration for unbound generic type {0}", implementationType.Name));

                if (!this.serviceType.IsGenericTypeDefinition)
                    throw new StyletIoCRegistrationException(String.Format("You may not bind the unbound generic type {0} to the bound generic / non-generic service {1}", implementationType.Name, this.serviceType.Name));

                // This restriction may change when I figure out how to pass down the correct type argument
                if (this.serviceType.GetTypeInfo().GenericTypeParameters.Length != implementationType.GetTypeInfo().GenericTypeParameters.Length)
                    throw new StyletIoCRegistrationException(String.Format("If you're registering an unbound generic type to an unbound generic service, both service and type must have the same number of type parameters. Service: {0}, Type: {1}", this.serviceType.Name, implementationType.Name));
            }
            else if (this.serviceType.IsGenericTypeDefinition)
            {
                throw new StyletIoCRegistrationException(String.Format("You cannot bind the bound generic / non-generic type {0} to unbound generic service {1}", implementationType.Name, this.serviceType.Name));
            }

            if (!implementationType.Implements(this.serviceType))
                throw new StyletIoCRegistrationException(String.Format("Type {0} does not implement service {1}", implementationType.Name, this.serviceType.Name));
        }

        // Convenience...
        protected void BindImplementationToService(StyletIoCContainer container, Type implementationType, Type serviceType = null)
        {
            serviceType = serviceType ?? this.serviceType;

            if (this.serviceType.IsGenericTypeDefinition)
            {
                var unboundGeneric = new UnboundGeneric(implementationType, container, this.isSingleton);
                container.AddUnboundGeneric(new TypeKey(serviceType, key), unboundGeneric);
            }
            else
            {
                var creator = new TypeCreator(implementationType, container);
                IRegistration registration = this.isSingleton ? (IRegistration)new SingletonRegistration(creator) : (IRegistration)new TransientRegistration(creator);
                container.AddRegistration(new TypeKey(this.serviceType, this.key ?? creator.AttributeKey), registration);
            }
        }

        void IWithKey.WithKey(string key)
        {
            this.key = key;
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
            this.EnsureType(typeof(TImplementation));
            if (this.serviceType.IsGenericTypeDefinition)
                throw new StyletIoCRegistrationException(String.Format("A factory cannot be used to implement unbound generic type {0}", this.serviceType.Name));
            this.factory = factory;
        }

        public override void Build(StyletIoCContainer container)
        {
            var creator = new FactoryCreator<TImplementation>(this.factory, container);
            IRegistration registration = this.isSingleton ? (IRegistration)new SingletonRegistration(creator) : (IRegistration)new TransientRegistration(creator);
            container.AddRegistration(new TypeKey(this.serviceType, this.key), registration);
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
            var candidates = from type in assemblies.SelectMany(x => x.GetTypes())
                             let baseType = type.GetBaseTypesAndInterfaces().FirstOrDefault(x => x == this.serviceType || x.IsGenericType && x.GetGenericTypeDefinition() == this.serviceType)
                             where baseType != null
                             select new { Type = type, Base = baseType.ContainsGenericParameters ? baseType.GetGenericTypeDefinition() : baseType };

            foreach (var candidate in candidates)
            {
                try
                {
                    this.EnsureType(candidate.Type);
                    this.BindImplementationToService(container, candidate.Type, candidate.Base);
                }
                catch (StyletIoCRegistrationException e)
                {
                    Debug.WriteLine(String.Format("Unable to auto-bind type {0} to {1}: {2}", candidate.Base.Name, candidate.Type.Name, e.Message), "StyletIoC");
                }
            }
        }
    }

    internal class AbstractFactoryBinding : BuilderBindingBase
    {
        public AbstractFactoryBinding(Type serviceType) : base(serviceType) { }

        public override void Build(StyletIoCContainer container)
        {
            var factoryType = container.GetFactoryForType(this.serviceType);
            var creator = new TypeCreator(factoryType, container);
            var registration = new SingletonRegistration(creator);
            container.AddRegistration(new TypeKey(this.serviceType, this.key), registration);
        }
    }

    public class StyletIoCBuilder
    {
        private List<BuilderBindTo> bindings = new List<BuilderBindTo>();

        public IBindTo Bind(Type serviceType)
        {
            var builderBindTo = new BuilderBindTo(serviceType);
            this.bindings.Add(builderBindTo);
            return builderBindTo;
        }

        public IBindTo Bind<TService>()
        {
            return this.Bind(typeof(TService));
        }

        public void Autobind(params Assembly[] assemblies)
        {
            // If they haven't given any assemblies, use the assembly of the caller
            if (assemblies == null || assemblies.Length == 0)
                assemblies = new[] { Assembly.GetCallingAssembly() };

            // We self-bind concrete classes only
            var classes = assemblies.SelectMany(x => x.GetTypes()).Where(c => c.IsClass && !c.IsAbstract);
            foreach (var cls in classes)
            {
                // Don't care if binding fails - we're likely to hit a few of these
                try
                {
                    this.Bind(cls).To(cls);
                }
                catch (StyletIoCRegistrationException e)
                {
                    Debug.WriteLine(String.Format("Unable to auto-bind type {0}: {1}", cls.Name, e.Message), "StyletIoC");
                }
            }
        }

        public IContainer BuildContainer()
        {
            var container = new StyletIoCContainer();
            container.AddRegistration(new TypeKey(typeof(IContainer), null), new SingletonRegistration(new FactoryCreator<StyletIoCContainer>(c => container, container)));
            container.AddRegistration(new TypeKey(typeof(StyletIoCContainer), null), new SingletonRegistration(new FactoryCreator<StyletIoCContainer>(c => container, container)));

            foreach (var binding in this.bindings)
            {
                binding.Build(container);
            }
            return container;
        }
    }
}
