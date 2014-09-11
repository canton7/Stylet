using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StyletIoC.Builder
{
    /// <summary>
    /// Delegate used to create an IRegistration
    /// </summary>
    /// <param name="creator">ICreator used by the IRegistration to create new instances</param>
    /// <param name="key">Key associated with the registration</param>
    /// <returns>A new IRegistration</returns>
    public delegate IRegistration RegistrationFactory(IRegistrationContext parentContext, ICreator creator, string key);

    /// <summary>
    /// An IRegistration is responsible to returning an appropriate (new or cached) instanced of a type, or an expression doing the same.
    /// It owns an ICreator, and will use it to create a new instance when needed.
    /// </summary>
    public interface IRegistration
    {
        /// <summary>
        /// Type of the object returned by the registration
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Fetches an instance of the relevaent type
        /// </summary>
        /// <returns>An object of type Type, which is supplied by the ICreator</returns>
        Func<IRegistrationContext, object> GetGenerator();

        /// <summary>
        /// Fetches an expression which evaluates to an instance of the relevant type
        /// </summary>
        /// <returns>An expression evaluating to an instance of type Type, which is supplied by the ICreator></returns>
        Expression GetInstanceExpression(ParameterExpression registrationContext);

        IRegistration CloneToContext(IRegistrationContext context);
    }
}
