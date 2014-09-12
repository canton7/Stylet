using StyletIoC.Creation;
using StyletIoC.Internal.Creators;
using System;

namespace StyletIoC.Internal
{
    internal class UnboundGeneric
    {
        private IRegistrationContext parentContext;
        private readonly Type serviceType;
        public Type Type { get; private set; }
        public RegistrationFactory RegistrationFactory { get; private set; }

        public UnboundGeneric(Type serviceType, Type type, IRegistrationContext parentContext, RegistrationFactory registrationFactory)
        {
            this.serviceType = serviceType;
            this.Type = type;
            this.parentContext = parentContext;
            this.RegistrationFactory = registrationFactory;
        }

        public IRegistration CreateRegistrationForTypeKey(TypeKey boundTypeKey)
        {
            return this.RegistrationFactory(this.parentContext, this.serviceType, new TypeCreator(boundTypeKey.Type, this.parentContext), boundTypeKey.Key);
        }
    }
}
