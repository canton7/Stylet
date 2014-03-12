using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace StyletUnitTests
{
    [TestFixture]
    public class BootstrapperBaseTests
    {
        private Application application;

        class TestBootstrapper : BootstrapperBase<object>
        {
            // Expose this
            public new Application Application { get { return base.Application; } }

            protected override object GetInstance(Type service, string key = null)
            {
                throw new NotImplementedException();
            }

            protected override IEnumerable<object> GetAllInstances(Type service)
            {
                throw new NotImplementedException();
            }

            protected override void BuildUp(object instance)
            {
                throw new NotImplementedException();
            }
        }

        [SetUp]
        public void SetUp()
        {
            this.application = new Application();
        }

        [Test]
        public void CapturesCurrentApplication()
        {
            var bootstrapper = new TestBootstrapper();
            Assert.AreEqual(this.application, bootstrapper.Application);
        }
    }
}
