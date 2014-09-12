using StyletIoC.Creation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletIoC.Internal.RegistrationCollections
{
    internal class EmptyRegistrationCollection : IRegistrationCollection
    {
        private readonly Type type;

        public EmptyRegistrationCollection(Type type)
        {
            this.type = type;
        }

        public IRegistration GetSingle()
        {
            throw new StyletIoCRegistrationException(String.Format("No registrations found for service {0}.", this.type.GetDescription()));
        }

        public List<IRegistration> GetAll()
        {
            return new List<IRegistration>();
        }

        public IRegistrationCollection AddRegistration(IRegistration registration)
        {
            return new SingleRegistration(registration);
        }

        public IRegistrationCollection CloneToContext(IRegistrationContext context)
        {
            return this;
        }
    }
}
