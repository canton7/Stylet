using StyletIoC.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StyletIoC.Internal.Registrations
{
    internal abstract class RegistrationBase : IRegistration
    {
        protected readonly ICreator creator;
        public Type Type { get { return this.creator.Type; } }

        private readonly object generatorLock = new object();
        protected Func<IRegistrationContext, object> generator;

        public RegistrationBase(ICreator creator)
        {
            this.creator = creator;
        }

        public virtual Func<IRegistrationContext, object> GetGenerator()
        {
            if (this.generator != null)
                return this.generator;

            lock (this.generatorLock)
            {
                if (this.generator == null)
                {
                    this.generator = this.GetGeneratorInternal();
                }
                return this.generator;
            }
        }

        protected virtual Func<IRegistrationContext, object> GetGeneratorInternal()
        {
            var registrationContext = Expression.Parameter(typeof(IRegistrationContext), "registrationContext");
            return Expression.Lambda<Func<IRegistrationContext, object>>(this.GetInstanceExpression(registrationContext), registrationContext).Compile();
        }

        public abstract Expression GetInstanceExpression(ParameterExpression registrationContext);

        public virtual IRegistration CloneToContext(IRegistrationContext context)
        {
            return this;
        }
    }
}
