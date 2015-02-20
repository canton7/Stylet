using StyletIoC.Creation;
using System.Collections.Generic;

namespace StyletIoC.Internal
{
    internal interface IRegistrationCollection : IReadOnlyRegistrationCollection
    {
        IRegistrationCollection AddRegistration(IRegistration registration);
    }

    internal interface IReadOnlyRegistrationCollection
    {
        IRegistration GetSingle();
        List<IRegistration> GetAll();
    }
}
