using StyletIoC.Creation;
using System;
using System.Linq.Expressions;

namespace StyletIoC.Internal.Creators
{
    /// <summary>
    /// Knows how to create an instance of a type, by using a Func{IRegistration, T} passed by the user during building
    /// </summary>
    /// <typeparam name="T">Type of object created by this factory</typeparam>
    internal class FactoryCreator<T> : CreatorBase
    {
        private readonly Func<IRegistrationContext, T> factory;

        public override RuntimeTypeHandle TypeHandle { get { return typeof(T).TypeHandle; } }

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
