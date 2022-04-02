using StyletIoC.Creation;
using System;
using System.Linq.Expressions;

namespace StyletIoC.Internal.Registrations;

/// <summary>
/// Convenience base class for all IRegistrations which want it
/// </summary>
internal abstract class RegistrationBase : IRegistration
{
    protected readonly ICreator Creator;
    public RuntimeTypeHandle TypeHandle => this.Creator.TypeHandle;

    protected readonly object LockObject = new();
    protected Func<IRegistrationContext, object> Generator;

    protected RegistrationBase(ICreator creator)
    {
        this.Creator = creator;
    }

    public virtual Func<IRegistrationContext, object> GetGenerator()
    {
        if (this.Generator != null)
            return this.Generator;

        lock (this.LockObject)
        {
            if (this.Generator == null)
                this.Generator = this.GetGeneratorInternal();
            return this.Generator;
        }
    }

    protected virtual Func<IRegistrationContext, object> GetGeneratorInternal()
    {
        ParameterExpression registrationContext = Expression.Parameter(typeof(IRegistrationContext), "registrationContext");
        return Expression.Lambda<Func<IRegistrationContext, object>>(this.GetInstanceExpression(registrationContext), registrationContext).Compile();
    }

    public abstract Expression GetInstanceExpression(ParameterExpression registrationContext);
}
