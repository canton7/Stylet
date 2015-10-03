using StyletIoC.Internal.Creators;
using StyletIoC.Internal.Registrations;
using System;
using StyletIoC.Creation;
using System.Collections.Generic;
using System.Diagnostics;

namespace StyletIoC.Internal.Builders
{
    internal class BuilderAbstractFactoryBinding : BuilderBindingBase
    {
        private BuilderTypeKey ServiceType { get { return this.ServiceTypes[0]; } }

        public BuilderAbstractFactoryBinding(List<BuilderTypeKey> serviceTypes)
            : base(serviceTypes)
        {
            // This should be ensured by the fluent interfaces
            Trace.Assert(serviceTypes.Count == 1);

            if (this.ServiceType.Type.IsGenericTypeDefinition)
                throw new StyletIoCRegistrationException(String.Format("Unbound generic type {0} can't be used as an abstract factory", this.ServiceType.Type.GetDescription()));
        }

        public override void Build(Container container)
        {
            var factoryType = container.GetFactoryForType(this.ServiceType.Type);
            var creator = new AbstractFactoryCreator(factoryType);
            var registration = new TransientRegistration(creator);

            container.AddRegistration(new TypeKey(this.ServiceType.Type.TypeHandle, this.ServiceType.Key), registration);
        }
    }
}
