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
        public List<BuilderTypeKey> ServiceTypes { get; private set; }
        private BuilderBindingBase builderBinding;
        public bool IsWeak { get { return this.builderBinding.IsWeak; } }

        public BuilderBindTo(Type serviceType, Func<IEnumerable<Assembly>, string, IEnumerable<Assembly>> getAssemblies)
        {
            this.ServiceTypes = new List<BuilderTypeKey>() { new BuilderTypeKey(serviceType) };
            this.getAssemblies = getAssemblies;
        }

        public IInScopeOrWithKeyOrAsWeakBinding ToSelf()
        {
            return this.To(this.ServiceTypes);
        }

        public IInScopeOrWithKeyOrAsWeakBinding To(Type implementationType)
        {
            this.builderBinding = new BuilderTypeBinding(this.ServiceTypes, implementationType);
            return this.builderBinding;
        }

        public IInScopeOrWithKeyOrAsWeakBinding To<TImplementation>()
        {
            return this.To(typeof(TImplementation));
        }

        public IInScopeOrWithKeyOrAsWeakBinding ToFactory<TImplementation>(Func<IRegistrationContext, TImplementation> factory)
        {
            this.builderBinding = new BuilderFactoryBinding<TImplementation>(this.ServiceTypes, factory);
            return this.builderBinding;
        }

        public IWithKeyOrAsWeakBinding ToInstance(object instance)
        {
            this.builderBinding = new BuilderInstanceBinding(this.ServiceTypes, instance);
            return this.builderBinding;
        }

        public IWithKeyOrAsWeakBinding ToAbstractFactory()
        {
            this.builderBinding = new BuilderAbstractFactoryBinding(this.ServiceTypes);
            return this.builderBinding;
        }

        public IInScopeOrWithKeyOrAsWeakBinding ToAllImplementations(IEnumerable<Assembly> assemblies)
        {
            this.builderBinding = new BuilderToAllImplementationsBinding(this.ServiceTypes, this.getAssemblies(assemblies, "ToAllImplementations"));
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
