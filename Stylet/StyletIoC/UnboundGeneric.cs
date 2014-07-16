using System;

namespace StyletIoC
{
    internal class UnboundGeneric
    {
        private StyletIoCContainer container;
        public Type Type { get; private set; }
        public Func<ICreator, IRegistration> RegistrationFactory { get; private set; }

        public UnboundGeneric(Type type, StyletIoCContainer container, Func<ICreator, IRegistration> registrationFactory)
        {
            this.Type = type;
            this.container = container;
            this.RegistrationFactory = registrationFactory;
        }

        public IRegistration CreateRegistrationForType(Type boundType)
        {
            return this.RegistrationFactory(new TypeCreator(boundType, this.container));
        }
    }
}
