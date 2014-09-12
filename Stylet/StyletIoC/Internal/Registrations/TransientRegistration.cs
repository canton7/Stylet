using StyletIoC.Creation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StyletIoC.Internal.Registrations
{
    internal class TransientRegistration : RegistrationBase
    {
        public TransientRegistration(ICreator creator) : base(creator) { }

        public override Expression GetInstanceExpression(System.Linq.Expressions.ParameterExpression registrationContext)
        {
            return this.creator.GetInstanceExpression(registrationContext);
        }
    }
}
