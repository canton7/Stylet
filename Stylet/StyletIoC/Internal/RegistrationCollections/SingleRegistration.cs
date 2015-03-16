using StyletIoC.Creation;
using System;
using System.Collections.Generic;

namespace StyletIoC.Internal.RegistrationCollections
{
    internal class SingleRegistration : IRegistrationCollection
    {
        private readonly IRegistration registration;

        public SingleRegistration(IRegistration registration)
        {
            this.registration = registration;
        }

        public IRegistration GetSingle()
        {
            return this.registration;
        }

        public List<IRegistration> GetAll()
        {
            return new List<IRegistration>() { this.registration };
        }

        public IRegistrationCollection AddRegistration(IRegistration registration)
        {
            if (this.registration.TypeHandle.Equals(registration.TypeHandle))
                throw new StyletIoCRegistrationException(String.Format("Multiple registrations for type {0} found.", Type.GetTypeFromHandle(registration.TypeHandle).GetDescription()));
            return new RegistrationCollection(new List<IRegistration>() { this.registration, registration });
        }
    }
}
