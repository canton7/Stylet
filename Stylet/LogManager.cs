using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Stylet
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
    public class DebugLogger : ILogger
    {
        private readonly string name;

        /// <summary>
        /// Create a new DebugLogger with the given name
        /// </summary>
        /// <param name="name">Name of the DebugLogger</param>
        public DebugLogger(string name)
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
            Debug.WriteLine(String.Format("[{1}] INFO {0}", String.Format(format, args), this.name), "Stylet");
        }

        /// <summary>
        /// Log the message as a warning
        /// </summary>
        /// <param name="format">A formatted message</param>
        /// <param name="args">format parameters</param>
        public void Warn(string format, params object[] args)
        {
            Debug.WriteLine(String.Format("[{1}] WARN {0}", String.Format(format, args), this.name), "Stylet");
        }

        /// <summary>
        /// Log an exception as an error
        /// </summary>
        /// <param name="exception">Exception to log</param>
        /// <param name="message">Additional message to add to the exception</param>
        public void Error(Exception exception, string message = null)
        {
            Debug.WriteLine(String.Format("[{2}] ERROR {0}: {1}", exception, message, this.name), "Stylet");
        }
    }

    /// <summary>
    /// Manager for ILoggers. Used to create new ILoggers, and set up how ILoggers are created
    /// </summary>
    public static class LogManager
    {
        private static readonly ILogger nullLogger = new NullLogger();

        /// <summary>
        /// Set to true to enable logging
        /// </summary>
        /// <remarks>
        /// When false (the default), a null logger will be returned by GetLogger().
        /// When true, LoggerFactory will be used to create a new logger
        /// </remarks>
        public static bool Enabled;

        /// <summary>
        /// Factory used to create new ILoggers, used by GetLogger
        /// </summary>
        /// <remarks>
        /// e.g. LogManager.LoggerFactory = name => new MyLogger(name);
        /// </remarks>
        public static Func<string, ILogger> LoggerFactory = name => new DebugLogger(name);

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
