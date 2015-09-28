using StyletIoC.Internal.Creators;
using StyletIoC.Internal.Registrations;
using System;
using StyletIoC.Creation;
using System.Collections.Generic;

namespace StyletIoC.Internal.Builders
{
    internal class BuilderAbstractFactoryBinding : BuilderBindingBase
    {
        public BuilderAbstractFactoryBinding(List<BuilderTypeKey> serviceTypes)
            : base(serviceTypes)
        {
            foreach (var serviceType in this.ServiceTypes)
            {
                if (serviceType.Type.IsGenericTypeDefinition)
                    throw new StyletIoCRegistrationException(String.Format("Unbound generic type {0} can't be used as an abstract factory", serviceType.Type.GetDescription()));
            }
        }

        public override void Build(Container container)
        {
            foreach (var serviceType in this.ServiceTypes)
            {
                var factoryType = container.GetFactoryForType(serviceType.Type);
                var creator = new AbstractFactoryCreator(factoryType);
                var registration = new TransientRegistration(creator);

                container.AddRegistration(new TypeKey(serviceType.Type.TypeHandle, serviceType.Key), registration);
            }
        }
    }
}
