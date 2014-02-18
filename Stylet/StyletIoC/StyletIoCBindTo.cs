using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StyletIoC
{
    public interface IStyletIoCBindTo
    {
        void ToSelf(string key = null);
        void To<TImplementation>(string key = null) where TImplementation : class;
        void To(Type implementationType, string key = null);
        void ToFactory<TImplementation>(Func<IContainer, TImplementation> factory, string key = null) where TImplementation : class;
        void ToAbstractFactory(string key = null);
        void ToAllImplementations(string key = null, params Assembly[] assembly);
    }

    public class StyletIoCBindTo : IStyletIoCBindTo
    {
        private StyletIoCContainer container;
        private Type serviceType;
        private bool isSingleton;

        public StyletIoCBindTo(StyletIoCContainer service, Type serviceType, bool isSingleton)
        {
            this.container = service;
            this.serviceType = serviceType;
            this.isSingleton = isSingleton;
        }

        public void ToSelf(string key = null)
        {
            this.To(this.serviceType, key);
        }

        public void To<TImplementation>(string key = null) where TImplementation : class
        {
            this.To(typeof(TImplementation), key);
        }

        public void To(Type implementationType, string key = null)
        {
            this.EnsureType(implementationType);
            if (this.serviceType.IsGenericTypeDefinition)
            {
                var unboundGeneric = new UnboundGeneric(implementationType, this.container, this.isSingleton);
                this.container.AddUnboundGeneric(new TypeKey(serviceType, key), unboundGeneric);
            }
            else
            {
                var creator = new TypeCreator(implementationType, this.container);
                this.AddRegistration(creator, implementationType, key ?? creator.AttributeKey);
            }
        }

        public void ToFactory<TImplementation>(Func<IContainer, TImplementation> factory, string key = null) where TImplementation : class
        {
            Type implementationType = typeof(TImplementation);
            this.EnsureType(implementationType);
            if (this.serviceType.IsGenericTypeDefinition)
                throw new StyletIoCRegistrationException(String.Format("A factory cannot be used to implement unbound generic type {0}", this.serviceType.Name));
            var creator = new FactoryCreator<TImplementation>(factory, this.container);
            this.AddRegistration(creator, implementationType, key);
        }

        public void ToAbstractFactory(string key = null)
        {
            var factoryType = this.container.GetFactoryForType(this.serviceType);
            this.To(factoryType, key);
        }

        public void ToAllImplementations(string key = null, params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
                assemblies = new[] { Assembly.GetCallingAssembly() };

            var candidates = from type in assemblies.SelectMany(x => x.GetTypes())
                                let baseType = type.GetBaseTypesAndInterfaces().FirstOrDefault(x => x == this.serviceType || x.IsGenericType && x.GetGenericTypeDefinition() == this.serviceType)
                                where baseType != null
                                select new { Type = type, Base = baseType.ContainsGenericParameters ? baseType.GetGenericTypeDefinition() : baseType };

            foreach (var candidate in candidates)
            {
                try
                {
                    this.container.Bind(candidate.Base).To(candidate.Type, key);
                }
                catch (StyletIoCRegistrationException e)
                {
                    Debug.WriteLine(String.Format("Unable to auto-bind type {0} to {1}: {2}", candidate.Base.Name, candidate.Type.Name, e.Message), "StyletIoC");
                }
            }
        }

        private void EnsureType(Type implementationType)
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

        private void AddRegistration(ICreator creator, Type implementationType, string key)
        {
            IRegistration registration;
            if (this.isSingleton)
                registration = new SingletonRegistration(creator);
            else
                registration = new TransientRegistration(creator);

            container.AddRegistration(new TypeKey(this.serviceType, key), registration);
        }
    }
}
