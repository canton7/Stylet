using StyletIoC.Creation;
using System;
using System.Linq.Expressions;
using System.Threading;

namespace StyletIoC.Internal.Registrations
{
    /// <summary>
    /// Registration which generates a single instance, and returns that instance thereafter
    /// </summary>
    internal class SingletonRegistration : RegistrationBase
    {
        private readonly IRegistrationContext parentContext;
        private Expression instanceExpression;
        private object instance;

        public SingletonRegistration(IRegistrationContext parentContext, ICreator creator)
            : base(creator)
        {
            this.parentContext = parentContext;
            this.parentContext.Disposing += (o, e) =>
            {
                var disposable = this.instance as IDisposable;
                if (disposable != null)
                    disposable.Dispose();

                this.instance = this.instanceExpression = null;
                this.ClearGenerator();
            };
        }

        public override Expression GetInstanceExpression(ParameterExpression registrationContext)
        {
            if (this.instanceExpression != null)
                return this.instanceExpression;

            this.instance = Expression.Lambda<Func<IRegistrationContext, object>>(this.Creator.GetInstanceExpression(registrationContext), registrationContext).Compile()(this.parentContext);

            // This expression yields the actual type of instance, not 'object'
            var instanceExpression = Expression.Constant(this.instance);
            Interlocked.CompareExchange(ref this.instanceExpression, instanceExpression, null);
            return this.instanceExpression;
        }
    }
}
