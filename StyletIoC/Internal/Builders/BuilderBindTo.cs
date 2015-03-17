using StyletIoC.Creation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StyletIoC.Internal.Builders
{
    internal class BuilderBindTo : IBindTo
    {
        private readonly Func<IEnumerable<Assembly>, string, IEnumerable<Assembly>> getAssemblies;
        public Type ServiceType { get; private set; }
        private BuilderBindingBase builderBinding;
        public bool IsWeak { get { return this.builderBinding.IsWeak; } }
        public string Key { get { return this.builderBinding.Key; } }

        public BuilderBindTo(Type serviceType, Func<IEnumerable<Assembly>, string, IEnumerable<Assembly>> getAssemblies)
        {
            this.ServiceType = serviceType;
            this.getAssemblies = getAssemblies;
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
            this.builderBinding = new BuilderToAllImplementationsBinding(this.ServiceType, this.getAssemblies(assemblies, "ToAllImplementations"));
            return this.builderBinding;
        }

        public IInScopeOrWithKeyOrAsWeakBinding ToAllImplementations(params Assembly[] assemblies)
        {
            return this.ToAllImplementations(assemblies.AsEnumerable());
        }

        internal void Build(Container container)
        {
            this.builderBinding.Build(container);
        }
    }
}
