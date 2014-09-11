using StyletIoC.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
