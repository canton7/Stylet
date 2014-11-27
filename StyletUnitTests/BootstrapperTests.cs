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
                base.Configure();
            }

            public bool ConfigureIoCCalled;
            protected override void ConfigureIoC(IStyletIoCBuilder builder)
            {
                this.ConfigureIoCCalled = true;
                builder.Bind<I1>().To<C1>();
                base.ConfigureIoC(builder);
            }

            public new object GetInstance(Type service, string key)
            {
                return base.GetInstance(service, key);
            }

            public new IEnumerable<object> GetAllInstances(Type service)
            {
                return base.GetAllInstances(service);
            }

            public new void BuildUp(object instance)
            {
                base.BuildUp(instance);
            }
        }

        private MyBootstrapper<RootViewModel> bootstrapper;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Execute.TestExecuteSynchronously = true;
            AssemblySource.Assemblies.Clear();
        }

        [SetUp]
        public void SetUp()
        {
            this.bootstrapper = new MyBootstrapper<RootViewModel>();
        }

        [Test]
        public void ConfigureBindsRequiredTypes()
        {
            AssemblySource.Assemblies.Add(this.GetType().Assembly);
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
            AssemblySource.Assemblies.Add(this.GetType().Assembly);
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

            container.Setup(x => x.Get(typeof(string), "test")).Returns("hello").Verifiable();
            var result = this.bootstrapper.GetInstance(typeof(string), "test");
            Assert.AreEqual("hello", result);
            container.Verify();
        }

        [Test]
        public void GetAllInstancesMappedToContainer()
        {
            var container = new Mock<IContainer>();
            this.bootstrapper.Container = container.Object;

            container.Setup(x => x.GetAll(typeof(int), null)).Returns(new object[] { 1, 2, 3 }).Verifiable();
            var result = this.bootstrapper.GetAllInstances(typeof(int));
            Assert.That(result, Is.EquivalentTo(new[] { 1, 2, 3 }));
            container.Verify();
        }

        [Test]
        public void BuildUpMappedToContainer()
        {
            var container = new Mock<IContainer>();
            this.bootstrapper.Container = container.Object;

            var instance = new object();
            container.Setup(x => x.BuildUp(instance)).Verifiable();
            this.bootstrapper.BuildUp(instance);
            container.Verify();
        }
    }
}
