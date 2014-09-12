using StyletIoC.Creation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StyletIoC.Internal.Registrations
{
    /// <summary>
    /// Registration which generates a single instance, and returns that instance thereafter
    /// </summary>
    internal class SingletonRegistration : RegistrationBase
    {
        private Expression instanceExpression;
        private object instance;
        private readonly IRegistrationContext parentContext;

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
                this.generator = null;
            };
        }

        public override Expression GetInstanceExpression(ParameterExpression registrationContext)
        {
            if (this.instanceExpression != null)
                return this.instanceExpression;

            this.instance = Expression.Lambda<Func<IRegistrationContext, object>>(this.creator.GetInstanceExpression(registrationContext), registrationContext).Compile()(this.parentContext);

            // This expression yields the actual type of instance, not 'object'
            var instanceExpression = Expression.Constant(this.instance);
            Interlocked.CompareExchange(ref this.instanceExpression, instanceExpression, null);
            return this.instanceExpression;
        }
    }
}
