using System;
using System.Collections.Generic;

namespace StyletIoC
{
    /// <summary>
    /// Describes an IoC container, specifically StyletIoC
    /// </summary>
    public interface IContainer : IDisposable
    {
        /// <summary>
        /// Compile all known bindings (which would otherwise be compiled when needed), checking the dependency graph for consistency
        /// </summary>
        /// <param name="throwOnError">If true, throw if we fail to compile a type</param>
        void Compile(bool throwOnError = true);

        /// <summary>
        /// Fetch a single instance of the specified type
        /// </summary>
        /// <param name="type">Type of service to fetch an implementation for</param>
        /// <param name="key">Key that implementations of the service to fetch were registered with, defaults to null</param>
        /// <returns>An instance of the requested service</returns>
        object Get(Type type, string key = null);

        /// <summary>
        /// Fetch a single instance of the specified type
        /// </summary>
        /// <typeparam name="T">Type of service to fetch an implementation for</typeparam>
        /// <param name="key">Key that implementations of the service to fetch were registered with, defaults to null</param>
        /// <returns>An instance of the requested service</returns>
        T Get<T>(string key = null);

        /// <summary>
        /// Fetch instances of all types which implement the specified service
        /// </summary>
        /// <param name="type">Type of the service to fetch implementations for</param>
        /// <param name="key">Key that implementations of the service to fetch were registered with, defaults to null</param>
        /// <returns>All implementations of the requested service, with the requested key</returns>
        IEnumerable<object> GetAll(Type type, string key = null);

        /// <summary>
        /// Fetch instances of all types which implement the specified service
        /// </summary>
        /// <typeparam name="T">Type of the service to fetch implementations for</typeparam>
        /// <param name="key">Key that implementations of the service to fetch were registered with, defaults to null</param>
        /// <returns>All implementations of the requested service, with the requested key</returns>
        IEnumerable<T> GetAll<T>(string key = null);

        /// <summary>
        /// If type is an IEnumerable{T} or similar, is equivalent to calling GetAll{T}. Else, is equivalent to calling Get{T}.
        /// </summary>
        /// <param name="type">If IEnumerable{T}, will fetch all implementations of T, otherwise wil fetch a single T</param>
        /// <param name="key">Key that implementations of the service to fetch were registered with, defaults to null</param>
        /// <returns>The resolved result</returns>
        object GetTypeOrAll(Type type, string key = null);

        /// <summary>
        /// If type is an IEnumerable{T} or similar, is equivalent to calling GetAll{T}. Else, is equivalent to calling Get{T}.
        /// </summary>
        /// <typeparam name="T">If IEnumerable{T}, will fetch all implementations of T, otherwise wil fetch a single T</typeparam>
        /// <param name="key">Key that implementations of the service to fetch were registered with, defaults to null</param>
        /// <returns>The resolved result</returns>
        T GetTypeOrAll<T>(string key = null);

        /// <summary>
        /// For each property/field with the [Inject] attribute, sets it to an instance of that type
        /// </summary>
        /// <param name="item">Item to build up</param>
        void BuildUp(object item);
    }
}
