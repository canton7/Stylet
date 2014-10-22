using StyletIoC.Creation;
using System;
using System.Linq.Expressions;

namespace StyletIoC.Internal.Registrations
{
    /// <summary>
    /// Convenience base class for all IRegistrations which want it
    /// </summary>
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
    }
}
