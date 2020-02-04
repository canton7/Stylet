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
        private object instance;

        public SingletonRegistration(IRegistrationContext parentContext, ICreator creator)
            : base(creator)
        {
            this.parentContext = parentContext;
            this.parentContext.Disposing += (o, e) =>
            {
                IDisposable disposable;
                lock (this.lockObject)
                {
                    disposable = this.instance as IDisposable;
                    this.instance = null;
                    this.generator = null;
                }
                if (disposable != null)
                    disposable.Dispose();
            };
        }

        public override Expression GetInstanceExpression(ParameterExpression registrationContext)
        {
            if (this.instance == null)
            {
                lock (this.lockObject)
                {
                    if (this.instance == null)
                        this.instance = Expression.Lambda<Func<IRegistrationContext, object>>(this.Creator.GetInstanceExpression(registrationContext), registrationContext).Compile()(this.parentContext);
                }
            }

            // This expression yields the actual type of instance, not 'object'
            var instanceExpression = Expression.Constant(this.instance);
            return instanceExpression;
        }
    }
}
