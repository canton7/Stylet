using System;

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
}
