using StyletIoC.Creation;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StyletIoC.Internal.RegistrationCollections
{
    internal class RegistrationCollection : IRegistrationCollection
    {
        private readonly object registrationsLock = new object();
        private readonly List<IRegistration> registrations;

        public RegistrationCollection(List<IRegistration> registrations)
        {
            this.registrations = registrations;
        }

        public IRegistration GetSingle()
        {
            throw new StyletIoCRegistrationException("Multiple registrations found.");
        }

        public List<IRegistration> GetAll()
        {
            List<IRegistration> registrationsCopy;
            lock (this.registrationsLock) { registrationsCopy = this.registrations.ToList(); }
            return registrationsCopy;
        }

        public IRegistrationCollection AddRegistration(IRegistration registration)
        {
            // Need to lock the list, as someone might be fetching from it while we do this
            lock (this.registrationsLock)
            {
                // Should have been caught by SingleRegistration.AddRegistration
                Debug.Assert(!this.registrations.Any(x => x.TypeHandle.Equals(registration.TypeHandle)));
                this.registrations.Add(registration);
                return this;
            }
        }
    }
}
