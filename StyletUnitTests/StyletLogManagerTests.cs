using Moq;
using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletUnitTests
{
    [TestFixture]
    public class StyletLogManagerTests
    {
        private Func<string, IStyletLogger> loggerFactory;

        [SetUp]
        public void SetUp()
        {
            StyletLogManager.Enabled = false;
            this.loggerFactory = StyletLogManager.LoggerFactory;
        }

        [TearDown]
        public void TearDown()
        {
            StyletLogManager.LoggerFactory = this.loggerFactory;
        }

        [Test]
        public void GetLoggerReturnsNullLoggerIfDisabled()
        {
            var logger = StyletLogManager.GetLogger("test");
            Assert.IsInstanceOf<NullLogger>(logger);
        }

        [Test]
        public void GetLoggerReturnsDebugLoggerByDefaultIfEnabled()
        {
            StyletLogManager.Enabled = true;
            var logger = StyletLogManager.GetLogger("test");
            Assert.IsInstanceOf<DebugLogger>(logger);
        }

        [Test]
        public void GetLoggerCallsFactoryToCreateLogger()
        {
            var logger = new Mock<IStyletLogger>();

            StyletLogManager.Enabled = true;
            StyletLogManager.LoggerFactory = name => logger.Object;
            Assert.AreEqual(logger.Object, StyletLogManager.GetLogger("test"));
        }

        [Test]
        public void GetLoggerPassesLoggerNameToLoggerFactory()
        {
            string loggerName = null;

            StyletLogManager.Enabled = true;
            StyletLogManager.LoggerFactory = name => { loggerName = name; return null; };

            StyletLogManager.GetLogger("testy");

            Assert.AreEqual("testy", loggerName);
        }

        [Test]
        public void GetLoggerGetsNameFromTypeIfTypeGiven()
        {
            string loggerName = null;

            StyletLogManager.Enabled = true;
            StyletLogManager.LoggerFactory = name => { loggerName = name; return null; };

            StyletLogManager.GetLogger(typeof(int));

            Assert.AreEqual("System.Int32", loggerName);
        }
    }
}
