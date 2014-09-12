using StyletIoC.Creation;
using System.Collections.Generic;

namespace StyletIoC.Internal
{
    internal interface IRegistrationCollection
    {
        IRegistration GetSingle();
        List<IRegistration> GetAll();
        IRegistrationCollection AddRegistration(IRegistration registration);
        IRegistrationCollection CloneToContext(IRegistrationContext context);
    }
}
