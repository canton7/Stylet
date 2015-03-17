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
        protected readonly ICreator Creator;
        public RuntimeTypeHandle TypeHandle { get { return this.Creator.TypeHandle; } }

        private readonly object generatorLock = new object();
        private Func<IRegistrationContext, object> generator;

        protected RegistrationBase(ICreator creator)
        {
            this.Creator = creator;
        }

        public virtual Func<IRegistrationContext, object> GetGenerator()
        {
            if (this.generator != null)
                return this.generator;

            lock (this.generatorLock)
            {
                this.generator = this.GetGeneratorInternal();
                return this.generator;
            }
        }

        protected virtual Func<IRegistrationContext, object> GetGeneratorInternal()
        {
            var registrationContext = Expression.Parameter(typeof(IRegistrationContext), "registrationContext");
            return Expression.Lambda<Func<IRegistrationContext, object>>(this.GetInstanceExpression(registrationContext), registrationContext).Compile();
        }

        protected void ClearGenerator()
        {
            lock (this.generatorLock)
            {
                this.generator = null;
            }
        }

        public abstract Expression GetInstanceExpression(ParameterExpression registrationContext);
    }
}
