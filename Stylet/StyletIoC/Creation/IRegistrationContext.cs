using System;
using System.Collections.Generic;

namespace StyletIoC.Creation
{
    /// <summary>
    /// Context used by IRegistration and ICreator to get things needed to create instances, etc
    /// </summary>
    public interface IRegistrationContext : IContainer
    {
        /// <summary>
        /// Get the BuilderUpper for the given type
        /// </summary>
        /// <remarks>
        /// A BuilderUpper is something which knows how to build up an object - that is, populate
        /// all parameters marked with [Inject].
        /// </remarks>
        /// <param name="type">The type of object to retrieve the BuilderUpper for</param>
        /// <returns>The appropriate BuilderUpper</returns>
        BuilderUpper GetBuilderUpper(Type type);

        /// <summary>
        /// Determine whether the container can resolve the given type+key combination
        /// </summary>
        /// <param name="type">Type to resolve</param>
        /// <param name="key">Key to resolve</param>
        /// <returns>True if the container can resolve this type+key combination</returns>
        bool CanResolve(Type type, string key);

        /// <summary>
        /// Retrieve a single IRegistration for the type+key combination, or throw an exception if non, or more than one, are avaiable
        /// </summary>
        /// <param name="type">Type to search for</param>
        /// <param name="key">Key to search for</param>
        /// <param name="searchGetAllTypes">
        /// If true, a Type of IEnumerableI{Something} can return a registration which can generate a List{ISomething},
        /// where each element in that list is a different instance implementing ISomething
        /// </param>
        /// <returns>The appropriate registration</returns>
        IRegistration GetSingleRegistration(Type type, string key, bool searchGetAllTypes);

        /// <summary>
        /// Retrieve all IRegistrations for the type+key combination
        /// </summary>
        /// <remarks>If a single registration exists, then the returned list will contain a single entry</remarks>
        /// <param name="type">Type to search for</param>
        /// <param name="key">Key to search for</param>
        /// <param name="searchGetAllTypes">
        /// If true, a Type of IEnumerableI{Something} can return a registration which can generate a List{ISomething},
        /// where each element in that list is a different instance implementing ISomething
        /// </param>
        /// <returns>The appropriate registrations</returns>
        IReadOnlyList<IRegistration> GetAllRegistrations(Type type, string key, bool searchGetAllTypes);

        /// <summary>
        /// Fired when Dispose is called on the container.
        /// Registrations which retain instances should dispose and release them when this event is fired
        /// </summary>
        event EventHandler Disposing;
    }
}
