using StyletIoC.Creation;
using StyletIoC.Internal.Creators;
using System;

namespace StyletIoC.Internal
{
    internal class UnboundGeneric
    {
        private readonly Type serviceType;
        private readonly IRegistrationContext parentContext;
        public Type Type { get; private set; }
        public RegistrationFactory RegistrationFactory { get; private set; }

        public UnboundGeneric(Type serviceType, Type type, IRegistrationContext parentContext, RegistrationFactory registrationFactory)
        {
            this.serviceType = serviceType;
            this.Type = type;
            this.parentContext = parentContext;
            this.RegistrationFactory = registrationFactory;
        }

        public IRegistration CreateRegistrationForTypeAndKey(Type boundType, string boundKey)
        {
            return this.RegistrationFactory(this.parentContext, this.serviceType, new TypeCreator(boundType, this.parentContext), boundKey);
        }
    }
}
