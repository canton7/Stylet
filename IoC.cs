using System;
using System.Collections.Generic;
using System.Linq;

namespace Stylet
{
    /// <summary>
    /// A lightweight wrapper around the IoC container of your choice. Configured in the bootstrapper
    /// </summary>
    public static class IoC
    {
        /// <summary>
        /// Assign this to a func which can get a single instance of a service, with a given key
        /// </summary>
        public static Func<Type, string, object> GetInstance = (service, key) => { throw new InvalidOperationException("IoC is not initialized"); };

        /// <summary>
        /// Assign this to a func which can get an IEnumerable of all instances of a service
        /// </summary>
        public static Func<Type, IEnumerable<object>> GetAllInstances = service => { throw new InvalidOperationException("IoC is not initialized"); };

        /// <summary>
        /// Assign this to a fun which can build up a given object
        /// </summary>
        public static Action<object> BuildUp = instance => { throw new InvalidOperationException("IoC is not initialized"); };

        /// <summary>
        /// Wraps GetInstance, adding typing
        /// </summary>
        public static T Get<T>(string key = null)
        {
            return (T)GetInstance(typeof(T), key);
        }

        /// <summary>
        /// Wraps GetAllInstances, adding typing
        /// </summary>
        public static IEnumerable<T> GetAll<T>()
        {
            return GetAllInstances(typeof(T)).Cast<T>();
        }
    }
}
