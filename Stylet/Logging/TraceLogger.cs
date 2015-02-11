using System;
using System.Diagnostics;

namespace Stylet.Logging
{
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
}
