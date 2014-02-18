using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletIoC
{
    internal interface IRegistrationCollection
    {
        IRegistration GetSingle();
        List<IRegistration> GetAll();
        IRegistrationCollection AddRegistration(IRegistration registration);
    }

    internal class SingleRegistration : IRegistrationCollection
    {
        private IRegistration registration;

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
            return new RegistrationCollection(new List<IRegistration>() { this.registration, registration });
        }
    }

    internal class RegistrationCollection : IRegistrationCollection
    {
        private List<IRegistration> registrations;

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
            lock (this.registrations) { registrationsCopy = registrations.ToList(); }
            return registrationsCopy;
        }

        public IRegistrationCollection AddRegistration(IRegistration registration)
        {
            // Need to lock the list, as someone might be fetching from it while we do this
            lock (this.registrations)
            {
                // Is there an existing registration for this type?
                var existingRegistration = this.registrations.FirstOrDefault(x => x.Type == registration.Type);
                if (existingRegistration != null)
                {
                    if (existingRegistration.WasAutoCreated)
                        this.registrations.Remove(existingRegistration);
                    else
                        throw new StyletIoCRegistrationException(String.Format("Multiple registrations for type {0} found.", registration.Type.Name));
                }
                this.registrations.Add(registration);
                return this;
            }
        }
    }
}
