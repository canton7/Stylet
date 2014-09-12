using System;
using System.Linq.Expressions;

namespace StyletIoC.Creation
{
    /// <summary>
    /// Delegate used to create an IRegistration
    /// </summary>
    /// <param name="parentContext">Context on which this registration will be created</param>
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

        /// <summary>
        /// When a child container is created, and a registration is needed, this method is called allowing the registration to do any transformations it needs
        /// </summary>
        /// <remarks>
        /// The registration should probably not mutate itself here, but return a new version of itself with the appropriate
        /// state set in an appropriate way.
        /// A good example is the PerChildRequestContainer, which uses this method to return a copy of itself which
        /// does not retain the instances that its parent does.
        /// 
        /// Most registrations simply return themselves here
        /// </remarks>
        /// <param name="context">New child container which needs this registration</param>
        /// <returns>Original registration, or modified copy</returns>
        IRegistration CloneToContext(IRegistrationContext context);
    }
}
