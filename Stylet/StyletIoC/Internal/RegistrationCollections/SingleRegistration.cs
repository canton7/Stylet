using StyletIoC.Builder;
using StyletIoC.Internal.RegistrationCollections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (this.registration.Type == registration.Type)
                throw new StyletIoCRegistrationException(String.Format("Multiple registrations for type {0} found.", registration.Type.Description()));
            return new RegistrationCollection(new List<IRegistration>() { this.registration, registration });
        }

        public IRegistrationCollection CloneToContext(IRegistrationContext context)
        {
            return new SingleRegistration(this.registration.CloneToContext(context));
        }
    }
}
