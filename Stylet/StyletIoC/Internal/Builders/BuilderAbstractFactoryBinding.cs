using StyletIoC.Internal.Creators;
using StyletIoC.Internal.Registrations;
using System;
using StyletIoC.Creation;
using System.Collections.Generic;
using System.Diagnostics;

namespace StyletIoC.Internal.Builders;

internal class BuilderAbstractFactoryBinding : BuilderBindingBase
{
    private BuilderTypeKey serviceType => this.ServiceTypes[0];

    public BuilderAbstractFactoryBinding(List<BuilderTypeKey> serviceTypes)
        : base(serviceTypes)
    {
        // This should be ensured by the fluent interfaces
        Trace.Assert(serviceTypes.Count == 1);

        if (this.serviceType.Type.IsGenericTypeDefinition)
            throw new StyletIoCRegistrationException(string.Format("Unbound generic type {0} can't be used as an abstract factory", this.serviceType.Type.GetDescription()));
    }

    public override void Build(Container container)
    {
        Type factoryType = container.GetFactoryForType(this.serviceType.Type);
        var creator = new AbstractFactoryCreator(factoryType);
        var registration = new TransientRegistration(creator);

        container.AddRegistration(new TypeKey(this.serviceType.Type.TypeHandle, this.serviceType.Key), registration);
    }
}
