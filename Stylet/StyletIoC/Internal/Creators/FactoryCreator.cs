using StyletIoC.Creation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StyletIoC.Internal.Creators
{
    // Sealed for consistency with TypeCreator
    internal sealed class FactoryCreator<T> : CreatorBase
    {
        private readonly Func<IRegistrationContext, T> factory;

        public override Type Type { get { return typeof(T); } }

        public FactoryCreator(Func<IRegistrationContext, T> factory, IRegistrationContext parentContext)
            : base(parentContext)
        {
            this.factory = factory;
        }

        public override Expression GetInstanceExpression(ParameterExpression registrationContext)
        {
            // Unfortunately we can't cache the result of this, as it relies on registrationContext
            var expr = (Expression<Func<IRegistrationContext, T>>)(ctx => this.factory(ctx));
            var invoked = Expression.Invoke(expr, registrationContext);

            var completeExpression = this.CompleteExpressionFromCreator(invoked, registrationContext);
            return completeExpression;
        }
    }
}
