using StyletIoC.Creation;
using System.Linq.Expressions;

namespace StyletIoC.Internal.Registrations
{
    /// <summary>
    /// Registration which generates a new instance each time one is requested
    /// </summary>
    internal class TransientRegistration : RegistrationBase
    {
        public TransientRegistration(ICreator creator) : base(creator) { }

        public override Expression GetInstanceExpression(ParameterExpression registrationContext)
        {
            return this.Creator.GetInstanceExpression(registrationContext);
        }
    }
}
