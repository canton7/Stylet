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
    internal class GetAllRegistration : IRegistration
    {
        private readonly IRegistrationContext parentContext;

        public string Key { get; private set; }
        private readonly Type _type;
        public Type Type
        {
            get { return this._type; }
        }

        private Expression expression;
        private readonly object generatorLock = new object();
        private Func<IRegistrationContext, object> generator;

        public GetAllRegistration(Type type, IRegistrationContext parentContext, string key)
        {
            this.Key = key;
            this._type = type;
            this.parentContext = parentContext;
        }

        public Func<IRegistrationContext, object> GetGenerator()
        {
            if (this.generator != null)
                return this.generator;

            lock (this.generatorLock)
            {
                if (this.generator == null)
                {
                    var registrationContext = Expression.Parameter(typeof(IRegistrationContext), "registrationContext");
                    this.generator = Expression.Lambda<Func<IRegistrationContext, object>>(this.GetInstanceExpression(registrationContext), registrationContext).Compile();
                }
                return this.generator;
            }
        }

        public Expression GetInstanceExpression(ParameterExpression registrationContext)
        {
            if (this.expression != null)
                return this.expression;

            var list = Expression.New(this.Type);
            var init = Expression.ListInit(list, this.parentContext.GetAllRegistrations(this.Type.GenericTypeArguments[0], this.Key, false).Select(x => x.GetInstanceExpression(registrationContext)));

            Interlocked.CompareExchange(ref this.expression, init, null);
            return this.expression;
        }

        public IRegistration CloneToContext(IRegistrationContext context)
        {
            throw new InvalidOperationException("should not be cloned");
        }
    }
}
