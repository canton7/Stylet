using System;
using System.Collections.Generic;
using System.Linq;

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
        /// Fetch instances of all types which implement the specified service
        /// </summary>
        /// <param name="type">Type of the service to fetch implementations for</param>
        /// <param name="key">Key that implementations of the service to fetch were registered with, defaults to null</param>
        /// <returns>All implementations of the requested service, with the requested key</returns>
        IEnumerable<object> GetAll(Type type, string key = null);

        /// <summary>
        /// If type is an IEnumerable{T} or similar, is equivalent to calling GetAll{T}. Else, is equivalent to calling Get{T}.
        /// </summary>
        /// <param name="type">If IEnumerable{T}, will fetch all implementations of T, otherwise wil fetch a single T</param>
        /// <param name="key">Key that implementations of the service to fetch were registered with, defaults to null</param>
        /// <returns></returns>
        object GetTypeOrAll(Type type, string key = null);

        /// <summary>
        /// For each property/field with the [Inject] attribute, sets it to an instance of that type
        /// </summary>
        /// <param name="item">Item to build up</param>
        void BuildUp(object item);

        /// <summary>
        /// Create a builder, which can create a child container (one which can have its own registrations and scope, but also shares everything from this container)
        /// </summary>
        /// <returns>A builder which can create a child container</returns>
        StyletIoCBuilder CreateChildBuilder();
    }

    /// <summary>
    /// Extension methods on IContainer
    /// </summary>
    public static class IContainerExtensions
    {
        /// <summary>
        /// Fetch a single instance of the specified type
        /// </summary>
        /// <typeparam name="T">Type of service to fetch an implementation for</typeparam>
        /// <param name="container">Container to get from</param>
        /// <param name="key">Key that implementations of the service to fetch were registered with, defaults to null</param>
        /// <returns>An instance of the requested service</returns>
        public static T Get<T>(this IContainer container, string key = null)
        {
            return (T)container.Get(typeof(T), key);
        }

        /// <summary>
        /// Fetch instances of all types which implement the specified service
        /// </summary>
        /// <typeparam name="T">Type of the service to fetch implementations for</typeparam>
        /// <param name="container">Container to get from</param>
        /// <param name="key">Key that implementations of the service to fetch were registered with, defaults to null</param>
        /// <returns>All implementations of the requested service, with the requested key</returns>
        public static IEnumerable<T> GetAll<T>(this IContainer container, string key = null)
        {
            return container.GetAll(typeof(T), key).Cast<T>();
        }

        /// <summary>
        /// If type is an IEnumerable{T} or similar, is equivalent to calling GetAll{T}. Else, is equivalent to calling Get{T}.
        /// </summary>
        /// <typeparam name="T">If IEnumerable{T}, will fetch all implementations of T, otherwise wil fetch a single T</typeparam>
        /// <param name="container">Container to get from</param>
        /// <param name="key">Key that implementations of the service to fetch were registered with, defaults to null</param>
        /// <returns></returns>
        public static T GetTypeOrAll<T>(this IContainer container, string key = null)
        {
            return (T)container.GetTypeOrAll(typeof(T), key);
        }
    }
}
