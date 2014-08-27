using System;

namespace StyletIoC
{
    internal class UnboundGeneric
    {
        private StyletIoCContainer container;
        public Type Type { get; private set; }
        public RegistrationFactory RegistrationFactory { get; private set; }

        public UnboundGeneric(Type type, StyletIoCContainer container, RegistrationFactory registrationFactory)
        {
            this.Type = type;
            this.container = container;
            this.RegistrationFactory = registrationFactory;
        }

        public IRegistration CreateRegistrationForTypeKey(TypeKey boundTypeKey)
        {
            return this.RegistrationFactory(new TypeCreator(boundTypeKey.Type, this.container), boundTypeKey.Key);
        }
    }
}
