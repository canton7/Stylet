using StyletIoC.Internal.Creators;
using StyletIoC.Internal.Registrations;
using System;

namespace StyletIoC.Internal.Builders
{
    internal class AbstractFactoryBinding : BuilderBindingBase
    {
        public AbstractFactoryBinding(Type serviceType)
            : base(serviceType)
        {
            if (serviceType.IsGenericTypeDefinition)
                throw new StyletIoCRegistrationException(String.Format("Unbound generic type {0} can't be used as an abstract factory", serviceType.GetDescription()));
        }

        public override void Build(Container container)
        {
            var factoryType = container.GetFactoryForType(this.serviceType);
            var creator = new AbstractFactoryCreator(factoryType);
            var registration = new TransientRegistration(creator);

            container.AddRegistration(new TypeKey(this.serviceType, this.Key), registration);
        }
    }
}
