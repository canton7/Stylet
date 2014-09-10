using System;

namespace StyletIoC
{
    internal class UnboundGeneric
    {
        private IRegistrationContext parentContext;
        public Type Type { get; private set; }
        public RegistrationFactory RegistrationFactory { get; private set; }

        public UnboundGeneric(Type type, IRegistrationContext parentContext, RegistrationFactory registrationFactory)
        {
            this.Type = type;
            this.parentContext = parentContext;
            this.RegistrationFactory = registrationFactory;
        }

        public IRegistration CreateRegistrationForTypeKey(TypeKey boundTypeKey)
        {
            return this.RegistrationFactory(this.parentContext, new TypeCreator(boundTypeKey.Type, this.parentContext), boundTypeKey.Key);
        }
    }
}
