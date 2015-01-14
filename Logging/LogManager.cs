using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Stylet.Logging
{
    /// <summary>
    /// Logger used by Stylet for internal logging
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Log the message as info
        /// </summary>
        /// <param name="format">A formatted message</param>
        /// <param name="args">format parameters</param>
        void Info(string format, params object[] args);

        /// <summary>
        /// Log the message as a warning
        /// </summary>
        /// <param name="format">A formatted message</param>
        /// <param name="args">format parameters</param>
        void Warn(string format, params object[] args);

        /// <summary>
        /// Log an exception as an error
        /// </summary>
        /// <param name="exception">Exception to log</param>
        /// <param name="message">Additional message to add to the exception</param>
        void Error(Exception exception, string message = null);
    }

    /// <summary>
    /// ILogger implementation which does nothing - used by default
    /// </summary>
    public class NullLogger : ILogger
    {
        /// <summary>
        /// Log the message as info
        /// </summary>
        /// <param name="format">A formatted message</param>
        /// <param name="args">format parameters</param>
        public void Info(string format, params object[] args) { }

        /// <summary>
        /// Log the message as a warning
        /// </summary>
        /// <param name="format">A formatted message</param>
        /// <param name="args">format parameters</param>
        public void Warn(string format, params object[] args) { }

        /// <summary>
        /// Log an exception as an error
        /// </summary>
        /// <param name="exception">Exception to log</param>
        /// <param name="message">Additional message to add to the exception</param>
        public void Error(Exception exception, string message = null) { }
    }

    /// <summary>
    /// ILogger implementation which uses Debug.WriteLine
    /// </summary>
    public class TraceLogger : ILogger
    {
        private readonly string name;

        /// <summary>
        /// Initialises a new instance of the <see cref="TraceLogger"/> class, with the given name
        /// </summary>
        /// <param name="name">Name of the DebugLogger</param>
        public TraceLogger(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Log the message as info
        /// </summary>
        /// <param name="format">A formatted message</param>
        /// <param name="args">format parameters</param>
        public void Info(string format, params object[] args)
        {
            Trace.WriteLine(String.Format("INFO [{1}] {0}", String.Format(format, args), this.name), "Stylet");
        }

        /// <summary>
        /// Log the message as a warning
        /// </summary>
        /// <param name="format">A formatted message</param>
        /// <param name="args">format parameters</param>
        public void Warn(string format, params object[] args)
        {
            Trace.WriteLine(String.Format("WARN [{1}] {0}", String.Format(format, args), this.name), "Stylet");
        }

        /// <summary>
        /// Log an exception as an error
        /// </summary>
        /// <param name="exception">Exception to log</param>
        /// <param name="message">Additional message to add to the exception</param>
        public void Error(Exception exception, string message = null)
        {
            if (message == null)
                Trace.WriteLine(String.Format("ERROR [{1}] {0}", exception, this.name), "Stylet");
            else
                Trace.WriteLine(String.Format("ERROR [{2}] {0} {1}", message, exception, this.name), "Stylet");
        }
    }

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
