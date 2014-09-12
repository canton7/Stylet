using StyletIoC.Creation;
using StyletIoC.Internal.Creators;
using StyletIoC.Internal.Registrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace StyletIoC.Internal.Builders
{
    internal abstract class BuilderBindingBase : IInScopeOrWithKeyOrAsWeakBinding, IWithKey
    {
        protected Type serviceType;
        protected RegistrationFactory registrationFactory;
        public string Key { get; protected set; }
        public bool IsWeak { get; protected set; }

        public BuilderBindingBase(Type serviceType)
        {
            this.serviceType = serviceType;

            // Default is transient
            this.registrationFactory = (ctx, creator, key) => new TransientRegistration(creator);
        }

        IAsWeakBinding IInScopeOrAsWeakBinding.WithRegistrationFactory(RegistrationFactory registrationFactory)
        {
            if (registrationFactory == null)
                throw new ArgumentNullException("registrationFactory");
            this.registrationFactory = registrationFactory;
            return this;
        }

        IInScopeOrAsWeakBinding IInScopeOrWithKeyOrAsWeakBinding.WithKey(string key)
        {
            this.Key = key;
            return this;
        }

        protected void EnsureType(Type implementationType, Type serviceType = null)
        {
            serviceType = serviceType ?? this.serviceType;

            if (!implementationType.IsClass || implementationType.IsAbstract)
                throw new StyletIoCRegistrationException(String.Format("Type {0} is not a concrete class, and so can't be used to implemented service {1}", implementationType.GetDescription(), serviceType.GetDescription()));

            // Test this first, as it's a bit clearer than hitting 'type doesn't implement service'
            if (implementationType.IsGenericTypeDefinition)
            {
                if (!serviceType.IsGenericTypeDefinition)
                    throw new StyletIoCRegistrationException(String.Format("You can't use an unbound generic type to implement anything that isn't an unbound generic service. Service: {0}, Type: {1}", serviceType.GetDescription(), implementationType.GetDescription()));

                // This restriction may change when I figure out how to pass down the correct type argument
                if (serviceType.GetTypeInfo().GenericTypeParameters.Length != implementationType.GetTypeInfo().GenericTypeParameters.Length)
                    throw new StyletIoCRegistrationException(String.Format("If you're registering an unbound generic type to an unbound generic service, both service and type must have the same number of type parameters. Service: {0}, Type: {1}", serviceType.GetDescription(), implementationType.GetDescription()));
            }
            else if (serviceType.IsGenericTypeDefinition)
            {
                if (implementationType.GetGenericArguments().Length > 0)
                    throw new StyletIoCRegistrationException(String.Format("You cannot bind the bound generic type {0} to the unbound generic service {1}", implementationType.GetDescription(), serviceType.GetDescription()));
                else
                    throw new StyletIoCRegistrationException(String.Format("You cannot bind the non-generic type {0} to the unbound generic service {1}", implementationType.GetDescription(), serviceType.GetDescription()));
            }

            if (!implementationType.Implements(this.serviceType))
                throw new StyletIoCRegistrationException(String.Format("Type {0} does not implement service {1}", implementationType.GetDescription(), serviceType.GetDescription()));
        }

        // Convenience...
        protected void BindImplementationToService(Container container, Type implementationType, Type serviceType = null)
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
                var registration = this.CreateRegistration(container, creator);

                container.AddRegistration(new TypeKey(serviceType, this.Key ?? creator.AttributeKey), registration);
            }
        }

        // Convenience...
        protected IRegistration CreateRegistration(IRegistrationContext registrationContext, ICreator creator)
        {
            return this.registrationFactory(registrationContext, creator, this.Key);
        }

        void IWithKey.WithKey(string key)
        {
            this.Key = key;
        }

        void IAsWeakBinding.AsWeakBinding()
        {
            this.IsWeak = true;
        }

        public abstract void Build(Container container);
    }
}
