using System;

namespace StyletIoC
{
    internal class UnboundGeneric
    {
        private StyletIoCContainer container;
        public Type Type { get; private set; }
        public bool IsSingleton { get; private set; }

        public UnboundGeneric(Type type, StyletIoCContainer container, bool isSingleton)
        {
            this.Type = type;
            this.container = container;
            this.IsSingleton = isSingleton;
        }

        public IRegistration CreateRegistrationForType(Type boundType)
        {
            if (this.IsSingleton)
                return new SingletonRegistration(new TypeCreator(boundType, this.container));
            else
                return new TransientRegistration(new TypeCreator(boundType, this.container));
        }
    }
}
