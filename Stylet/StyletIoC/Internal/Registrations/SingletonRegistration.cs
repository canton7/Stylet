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
    internal class SingletonRegistration : RegistrationBase
    {
        private Expression instanceExpression;
        private object instance;
        private readonly IRegistrationContext parentContext;
        private bool disposed = false;

        public SingletonRegistration(IRegistrationContext parentContext, ICreator creator)
            : base(creator)
        {
            this.parentContext = parentContext;
            this.parentContext.Disposing += (o, e) =>
            {
                this.disposed = true;

                var disposable = this.instance as IDisposable;
                if (disposable != null)
                    disposable.Dispose();

                this.instance = null;
                this.instanceExpression = null;
                this.generator = null;
            };
        }

        public override Expression GetInstanceExpression(ParameterExpression registrationContext)
        {
            if (this.disposed)
                throw new ObjectDisposedException(String.Format("Singleton registration for type {0}", this.Type.GetDescription()));

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
