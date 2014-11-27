using StyletIoC.Creation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StyletIoC.Internal.Builders
{
    internal class BuilderBindTo : IBindTo
    {
        public Type ServiceType { get; private set; }
        private BuilderBindingBase builderBinding;
        public bool IsWeak { get { return this.builderBinding.IsWeak; } }
        public string Key { get { return this.builderBinding.Key; } }

        public BuilderBindTo(Type serviceType)
        {
            this.ServiceType = serviceType;
        }

        public IInScopeOrWithKeyOrAsWeakBinding ToSelf()
        {
            return this.To(this.ServiceType);
        }

        public IInScopeOrWithKeyOrAsWeakBinding To(Type implementationType)
        {
            this.builderBinding = new BuilderTypeBinding(this.ServiceType, implementationType);
            return this.builderBinding;
        }

        public IInScopeOrWithKeyOrAsWeakBinding To<TImplementation>()
        {
            return this.To(typeof(TImplementation));
        }

        public IInScopeOrWithKeyOrAsWeakBinding ToFactory<TImplementation>(Func<IRegistrationContext, TImplementation> factory)
        {
            this.builderBinding = new BuilderFactoryBinding<TImplementation>(this.ServiceType, factory);
            return this.builderBinding;
        }

        public IWithKeyOrAsWeakBinding ToInstance(object instance)
        {
            this.builderBinding = new BuilderInstanceBinding(this.ServiceType, instance);
            return this.builderBinding;
        }

        public IWithKeyOrAsWeakBinding ToAbstractFactory()
        {
            this.builderBinding = new BuilderAbstractFactoryBinding(this.ServiceType);
            return this.builderBinding;
        }

        public IInScopeOrWithKeyOrAsWeakBinding ToAllImplementations(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null || !assemblies.Any())
                assemblies = new[] { Assembly.GetCallingAssembly() };
            this.builderBinding = new BuilderToAllImplementationsBinding(this.ServiceType, assemblies);
            return this.builderBinding;
        }

        public IInScopeOrWithKeyOrAsWeakBinding ToAllImplementations(params Assembly[] assemblies)
        {
            // Have to do null-or-empty check here as well, otherwise GetCallingAssembly returns this one....
            if (assemblies == null || assemblies.Length == 0)
                assemblies = new[] { Assembly.GetCallingAssembly() };
            return this.ToAllImplementations(assemblies.AsEnumerable());
        }

        internal void Build(Container container)
        {
            this.builderBinding.Build(container);
        }
    }
}
