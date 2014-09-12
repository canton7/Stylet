using StyletIoC.Creation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StyletIoC.Internal.Registrations
{
    /// <summary>
    /// Knows how to create a Func{T}, using a given IRegistration
    /// </summary>
    // We're only created when we're needed, so no point in trying to be lazy
    internal class FuncNoKeyRegistration : IRegistration
    {
        private IRegistration delegateRegistration;
        private readonly Type funcType;
        private readonly Func<IRegistrationContext, object> generator;

        public Type Type
        {
            get { return this.funcType; }
        }

        public FuncNoKeyRegistration(IRegistration delegateRegistration)
        {
            this.delegateRegistration = delegateRegistration;
            this.funcType = Expression.GetFuncType(this.delegateRegistration.Type);

            var registrationContext = Expression.Parameter(typeof(IRegistrationContext), "registrationContext");
            this.generator = Expression.Lambda<Func<IRegistrationContext, object>>(this.GetInstanceExpression(registrationContext), registrationContext).Compile();
        }

        public Func<IRegistrationContext, object> GetGenerator()
        {
            return this.generator;
        }

        public Expression GetInstanceExpression(ParameterExpression registrationContext)
        {
            return Expression.Lambda(this.delegateRegistration.GetInstanceExpression(registrationContext));
        }

        public IRegistration CloneToContext(IRegistrationContext context)
        {
            return this;
        }
    }
}
