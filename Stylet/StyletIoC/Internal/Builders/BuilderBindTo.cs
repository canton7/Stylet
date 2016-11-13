using StyletIoC.Creation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace StyletIoC.Internal.Builders
{
    internal class BuilderBindTo : IBindTo, IAndOrToMultipleServices
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

        public IWithKeyOrAndOrToMultipleServices And<TService>()
        {
            return this.And(typeof(TService));
        }

        public IWithKeyOrAndOrToMultipleServices And(Type serviceType)
        {
            this.ServiceTypes.Add(new BuilderTypeKey(serviceType));
            return this;
        }

        public IAndOrToMultipleServices WithKey(string key)
        {
            // Should have been ensured by the fluent interface
            Trace.Assert(this.ServiceTypes.Count > 0);

            this.ServiceTypes[this.ServiceTypes.Count - 1].Key = key;
            return this;
        }

        public IInScopeOrWithKeyOrAsWeakBinding ToSelf()
        {
            // This should be ensured by the fluent interfaces
            Trace.Assert(this.ServiceTypes.Count == 1);

            return this.To(this.ServiceTypes[0].Type);
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

        public IWithKeyOrAsWeakBindingOrDisposeWithContainer ToInstance(object instance)
        {
            var builderBinding = new BuilderInstanceBinding(this.ServiceTypes, instance);
            this.builderBinding = builderBinding;
            return builderBinding;
        }

        public IWithKeyOrAsWeakBinding ToAbstractFactory()
        {
            this.builderBinding = new BuilderAbstractFactoryBinding(this.ServiceTypes);
            return this.builderBinding;
        }

        public IInScopeOrWithKeyOrAsWeakBinding ToAllImplementations(IEnumerable<Assembly> assemblies, bool allowZeroImplementations = false)
        {
            this.builderBinding = new BuilderToAllImplementationsBinding(this.ServiceTypes, this.getAssemblies(assemblies, "ToAllImplementations"), allowZeroImplementations);
            return this.builderBinding;
        }

        public IInScopeOrWithKeyOrAsWeakBinding ToAllImplementations(bool allowZeroImplementations = false, params Assembly[] assemblies)
        {
            return this.ToAllImplementations(assemblies.AsEnumerable(), allowZeroImplementations);
        }

        internal void Build(Container container)
        {
            this.builderBinding.Build(container);
        }
    }
}
