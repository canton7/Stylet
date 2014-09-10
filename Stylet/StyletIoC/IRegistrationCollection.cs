using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StyletIoC
{
    public interface IRegistrationCollection
    {
        IRegistration GetSingle();
        List<IRegistration> GetAll();
        IRegistrationCollection AddRegistration(IRegistration registration);
    }

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
    }

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
            lock (this.registrationsLock) { registrationsCopy = registrations.ToList(); }
            return registrationsCopy;
        }

        public IRegistrationCollection AddRegistration(IRegistration registration)
        {
            // Need to lock the list, as someone might be fetching from it while we do this
            lock (this.registrationsLock)
            {
                // Should have been caught by SingleRegistration.AddRegistration
                Debug.Assert(!this.registrations.Any(x => x.Type == registration.Type));
                this.registrations.Add(registration);
                return this;
            }
        }
    }
}
