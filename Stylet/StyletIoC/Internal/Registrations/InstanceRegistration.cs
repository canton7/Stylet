using StyletIoC.Creation;
using System;
using System.Linq.Expressions;

namespace StyletIoC.Internal.Registrations
{
    internal class InstanceRegistration : IRegistration
    {
        public RuntimeTypeHandle TypeHandle { get; private set; }
        private readonly object instance;
        private readonly Expression instanceExpression;

        public InstanceRegistration(IRegistrationContext parentContext, object instance, bool disposeWithContainer)
        {
            var type = instance.GetType();
            this.TypeHandle = type.TypeHandle;
            this.instance = instance;
            this.instanceExpression = Expression.Constant(instance, type);

            if (disposeWithContainer)
            {
                parentContext.Disposing += (o, e) =>
                {
                    var disposable = this.instance as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                };
            }
        }

        public Expression GetInstanceExpression(ParameterExpression registrationContext)
        {
            return this.instanceExpression;
        }

        public Func<IRegistrationContext, object> GetGenerator()
        {
            return x => this.instance;
        }
    }
}
