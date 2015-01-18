using System;

namespace Stylet.Logging
{
    /// <summary>
    /// Manager for ILoggers. Used to create new ILoggers, and set up how ILoggers are created
    /// </summary>
    public static class LogManager
    {
        private static readonly ILogger nullLogger = new NullLogger();

        /// <summary>
        /// Gets or sets a value indicating whether logging is enabled
        /// </summary>
        /// <remarks>
        /// When false (the default), a null logger will be returned by GetLogger().
        /// When true, LoggerFactory will be used to create a new logger
        /// </remarks>
        public static bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the factory used to create new ILoggers, used by GetLogger
        /// </summary>
        /// <example>
        /// LogManager.LoggerFactory = name => new MyLogger(name);
        /// </example>
        public static Func<string, ILogger> LoggerFactory { get; set; }

        static LogManager()
        {
            LoggerFactory = name => new TraceLogger(name);
        }

        /// <summary>
        /// Get a new ILogger for the given type
        /// </summary>
        /// <param name="type">Type which is using the ILogger</param>
        /// <returns>ILogger for use by the given type</returns>
        public static ILogger GetLogger(Type type)
        {
            return GetLogger(type.FullName);
        }

        /// <summary>
        /// Get a new ILogger with the given name
        /// </summary>
        /// <param name="name">Name of the ILogger</param>
        /// <returns>ILogger with the given name</returns>
        public static ILogger GetLogger(string name)
        {
            return Enabled ? LoggerFactory(name) : nullLogger;
        }
    }
}
