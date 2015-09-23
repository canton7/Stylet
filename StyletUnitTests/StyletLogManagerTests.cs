using Moq;
using NUnit.Framework;
using Stylet.Logging;
using System;

namespace StyletUnitTests
{
    [TestFixture]
    public class LogManagerTests
    {
        private Func<string, ILogger> loggerFactory;

        [SetUp]
        public void SetUp()
        {
            LogManager.Enabled = false;
            this.loggerFactory = LogManager.LoggerFactory;
        }

        [TearDown]
        public void TearDown()
        {
            LogManager.LoggerFactory = this.loggerFactory;
        }

        [Test]
        public void GetLoggerReturnsNullLoggerIfDisabled()
        {
            var logger = LogManager.GetLogger("test");
            Assert.IsInstanceOf<NullLogger>(logger);
        }

        [Test]
        public void GetLoggerReturnsDebugLoggerByDefaultIfEnabled()
        {
            LogManager.Enabled = true;
            var logger = LogManager.GetLogger("test");
            Assert.IsInstanceOf<TraceLogger>(logger);
        }

        [Test]
        public void GetLoggerCallsFactoryToCreateLogger()
        {
            var logger = new Mock<ILogger>();

            LogManager.Enabled = true;
            LogManager.LoggerFactory = name => logger.Object;
            Assert.AreEqual(logger.Object, LogManager.GetLogger("test"));
        }

        [Test]
        public void GetLoggerPassesLoggerNameToLoggerFactory()
        {
            string loggerName = null;

            LogManager.Enabled = true;
            LogManager.LoggerFactory = name => { loggerName = name; return null; };

            LogManager.GetLogger("testy");

            Assert.AreEqual("testy", loggerName);
        }

        [Test]
        public void GetLoggerGetsNameFromTypeIfTypeGiven()
        {
            string loggerName = null;

            LogManager.Enabled = true;
            LogManager.LoggerFactory = name => { loggerName = name; return null; };

            LogManager.GetLogger(typeof(int));

            Assert.AreEqual("System.Int32", loggerName);
        }
    }
}
