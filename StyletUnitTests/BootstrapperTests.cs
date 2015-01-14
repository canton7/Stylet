using Moq;
using NUnit.Framework;
using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StyletUnitTests
{
    [TestFixture]
    public class BootstrapperTests
    {
        private interface I1 { }
        private class C1 : I1 { }

        private class RootViewModel { }

        private class MyBootstrapper<T> : Bootstrapper<T> where T : class
        {
            public new IContainer Container
            {
                get { return base.Container; }
                set { base.Container = value; }
            }

            public new void Configure()
            {
                base.ConfigureBootstrapper();
            }

            public bool ConfigureIoCCalled;
            protected override void ConfigureIoC(IStyletIoCBuilder builder)
            {
                this.ConfigureIoCCalled = true;
                builder.Bind<I1>().To<C1>();
                base.ConfigureIoC(builder);
            }

            public new void OnExitInternal(ExitEventArgs e)
            {
                base.OnExitInternal(e);
            }
        }

        private MyBootstrapper<RootViewModel> bootstrapper;

        [SetUp]
        public void SetUp()
        {
            this.bootstrapper = new MyBootstrapper<RootViewModel>();
        }

        [Test]
        public void ConfigureBindsRequiredTypes()
        {
            this.bootstrapper.Configure();
            var ioc = this.bootstrapper.Container;

            Assert.IsInstanceOf<WindowManager>(ioc.Get<IWindowManager>());
            Assert.IsInstanceOf<IEventAggregator>(ioc.Get<IEventAggregator>());
            Assert.IsInstanceOf<ViewManager>(ioc.Get<IViewManager>());

            // Test autobinding
            Assert.DoesNotThrow(() => ioc.Get<RootViewModel>());
        }

        [Test]
        public void ConfigureCallsConfigureIoCWithCorrectBuilder()
        {
            this.bootstrapper.Configure();
            var ioc = this.bootstrapper.Container;

            Assert.True(this.bootstrapper.ConfigureIoCCalled);
            Assert.IsInstanceOf<C1>(ioc.Get<I1>());
        }

        [Test]
        public void GetInstanceMappedToContainer()
        {
            var container = new Mock<IContainer>();
            this.bootstrapper.Container = container.Object;

            container.Setup(x => x.Get(typeof(string), null)).Returns("hello").Verifiable();
            var result = this.bootstrapper.GetInstance(typeof(string));
            Assert.AreEqual("hello", result);
            container.Verify();
        }

        [Test]
        public void OnExitInternalDisposesContainer()
        {
            var container = new Mock<IContainer>();
            this.bootstrapper.Container = container.Object;
            this.bootstrapper.OnExitInternal(null);
            container.Verify(x => x.Dispose());
        }
    }
}
