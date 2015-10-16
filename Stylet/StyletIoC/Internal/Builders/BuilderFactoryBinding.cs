using StyletIoC.Creation;
using StyletIoC.Internal.Creators;
using System;
using System.Collections.Generic;

namespace StyletIoC.Internal.Builders
{
    internal class BuilderFactoryBinding<TImplementation> : BuilderBindingBase
    {
        private readonly Func<IRegistrationContext, TImplementation> factory;

        public BuilderFactoryBinding(List<BuilderTypeKey> serviceTypes, Func<IRegistrationContext, TImplementation> factory)
            : base(serviceTypes)
        {
            foreach (var serviceType in this.ServiceTypes)
            {
                if (serviceType.Type.IsGenericTypeDefinition)
                    throw new StyletIoCRegistrationException(String.Format("A factory cannot be used to implement unbound generic type {0}", serviceType.Type.GetDescription()));
                this.EnsureTypeAgainstServiceTypes(typeof(TImplementation), assertImplementation: false);
            }
            this.factory = factory;
        }

        public override void Build(Container container)
        {
            var creator = new FactoryCreator<TImplementation>(this.factory, container);
            var registration = this.CreateRegistration(container, creator);

            foreach (var serviceType in this.ServiceTypes)
            {
                container.AddRegistration(new TypeKey(serviceType.Type.TypeHandle, serviceType.Key), registration);
            }
        }
    }
}
