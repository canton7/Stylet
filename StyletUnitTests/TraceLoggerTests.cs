using NUnit.Framework;
using Stylet.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StyletUnitTests
{
    [TestFixture]
    public class TraceLoggerTests
    {
        private class StringTraceListener : TraceListener
        {
            public readonly List<string> messages = new List<string>();

            public override void Write(string message)
            {
                throw new NotImplementedException();
            }

            public override void WriteLine(string message)
            {
                this.messages.Add(message);
            }
        }

        private StringTraceListener listener;

        [SetUp]
        public void SetUp()
        {
            this.listener = new StringTraceListener();
            Trace.Listeners.Clear();
            Trace.Listeners.Add(this.listener);
        }

        [Test]
        public void InfoWritesAppropriateString()
        {
            var logger = new TraceLogger("testName");
            logger.Info("{0} message", "test");

            Assert.AreEqual(1, this.listener.messages.Count);
            var message = this.listener.messages[0];

            Assert.That(message, Contains.Substring("INFO")); // Log level
            Assert.That(message, Contains.Substring("Stylet")); // Category
            Assert.That(message, Contains.Substring("testName")); // Name
            Assert.That(message, Contains.Substring("test message")); // Actual message
        }

        [Test]
        public void WarnWritesAppropriateString()
        {
            var logger = new TraceLogger("loggerName");
            logger.Warn("this is a {0} message", "test");

            Assert.AreEqual(1, this.listener.messages.Count);
            var message = this.listener.messages[0];

            Assert.That(message, Contains.Substring("WARN")); // Log level
            Assert.That(message, Contains.Substring("Stylet")); // Category
            Assert.That(message, Contains.Substring("loggerName")); // Name
            Assert.That(message, Contains.Substring("this is a test message")); // Actual message
        }

        [Test]
        public void ErrorWithMessageWritesAppropriateString()
        {
            var logger = new TraceLogger("loggerWithErrorName");
            var e = new Exception("exception message");
            logger.Error(e, "accompanying message");

            Assert.AreEqual(1, this.listener.messages.Count);
            var message = this.listener.messages[0];

            Assert.That(message, Contains.Substring("ERROR")); // Log level
            Assert.That(message, Contains.Substring("Stylet")); // Category
            Assert.That(message, Contains.Substring("loggerWithErrorName")); // Name
            Assert.That(message, Contains.Substring("exception message")); // Exception message
            Assert.That(message, Contains.Substring("accompanying message"));
        }

        [Test]
        public void ErrorWithoutMessageWritesAppropriateString()
        {
            var logger = new TraceLogger("loggerWithErrorName");
            var e = new Exception("exception message");
            logger.Error(e);

            Assert.AreEqual(1, this.listener.messages.Count);
            var message = this.listener.messages[0];

            Assert.That(message, Contains.Substring("ERROR")); // Log level
            Assert.That(message, Contains.Substring("Stylet")); // Category
            Assert.That(message, Contains.Substring("loggerWithErrorName")); // Name
            Assert.That(message, Contains.Substring("exception message")); // Exception message
        }
    }
}
