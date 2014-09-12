using StyletIoC.Creation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

        public IInScopeOrWithKeyOrAsWeakBinding ToFactory<TImplementation>(Func<IRegistrationContext, TImplementation> factory)
        {
            this.builderBinding = new BuilderFactoryBinding<TImplementation>(this.ServiceType, factory);
            return this.builderBinding;
        }

        public IWithKey ToAbstractFactory()
        {
            this.builderBinding = new AbstractFactoryBinding(this.ServiceType);
            return this.builderBinding;
        }

        public IInScopeOrWithKeyOrAsWeakBinding ToAllImplementations(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null || !assemblies.Any())
                assemblies = new[] { Assembly.GetCallingAssembly() };
            this.builderBinding = new BuilderToAllImplementationsBinding(this.ServiceType, assemblies);
            return this.builderBinding;
        }

        internal void Build(Container container)
        {
            this.builderBinding.Build(container);
        }
    }
}
