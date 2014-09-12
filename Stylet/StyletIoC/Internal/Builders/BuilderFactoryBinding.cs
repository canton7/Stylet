using StyletIoC.Creation;
using StyletIoC.Internal.Creators;
using System;

namespace StyletIoC.Internal.Builders
{
    internal class BuilderFactoryBinding<TImplementation> : BuilderBindingBase
    {
        private readonly Func<IRegistrationContext, TImplementation> factory;

        public BuilderFactoryBinding(Type serviceType, Func<IRegistrationContext, TImplementation> factory)
            : base(serviceType)
        {
            if (this.serviceType.IsGenericTypeDefinition)
                throw new StyletIoCRegistrationException(String.Format("A factory cannot be used to implement unbound generic type {0}", this.serviceType.GetDescription()));
            this.EnsureType(typeof(TImplementation));
            this.factory = factory;
        }

        public override void Build(Container container)
        {
            var creator = new FactoryCreator<TImplementation>(this.factory, container);
            var registration = this.CreateRegistration(container, creator);

            container.AddRegistration(new TypeKey(this.serviceType, this.Key), registration);
        }
    }
}
