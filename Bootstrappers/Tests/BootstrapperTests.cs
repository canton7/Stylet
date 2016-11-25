using NUnit.Framework;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bootstrappers.Tests
{
    public class TestRootViewModel { }

    public interface ITestBootstrapper
    {
        int DisposeCount { get; }
        object GetInstance(Type type);
        void ConfigureBootstrapper();
        List<string> ConfigureLog { get; }
    }

    public abstract class BootstrapperTests<TBootstrapper> where TBootstrapper : ITestBootstrapper, IDisposable
    {
        protected TBootstrapper bootstrapper;

        public abstract TBootstrapper CreateBootstrapper();
        protected virtual bool Autobinds { get; set; }

        [SetUp]
        public void SetUp()
        {
            this.bootstrapper = this.CreateBootstrapper();
            this.bootstrapper.ConfigureBootstrapper();
        }

        [Test]
        public void CallsConfiguredInCorrectOrder()
        {
            Assert.That(this.bootstrapper.ConfigureLog, Is.EquivalentTo(new[] { "DefaultConfigureIoC", "ConfigureIoC" }));
        }

        [Test]
        public void ReturnsCorrectViewManager()
        {
            var vm = this.bootstrapper.GetInstance(typeof(IViewManager));
            Assert.IsInstanceOf<ViewManager>(vm);
        }

        [Test]
        public void ReturnsSingletonViewManager()
        {
            var vm1 = this.bootstrapper.GetInstance(typeof(IViewManager));
            var vm2 = this.bootstrapper.GetInstance(typeof(IViewManager));
            Assert.AreEqual(vm1, vm2);
        }

        [Test]
        public void ReturnsCorrectWindowManager()
        {
            var wm = this.bootstrapper.GetInstance(typeof(IWindowManager));
            Assert.IsInstanceOf<WindowManager>(wm);
        }

        [Test]
        public void ReturnsSingletonWindowManager()
        {
            var wm1 = this.bootstrapper.GetInstance(typeof(IWindowManager));
            var wm2 = this.bootstrapper.GetInstance(typeof(IWindowManager));
            Assert.AreEqual(wm1, wm2);
        }

        [Test]
        public void ReturnsCorrectEventAggregator()
        {
            var ea = this.bootstrapper.GetInstance(typeof(IEventAggregator));
            Assert.IsInstanceOf<EventAggregator>(ea);
        }

        [Test]
        public void ReturnsSingletonEventAggregator()
        {
            var ea1 = this.bootstrapper.GetInstance(typeof(IEventAggregator));
            var ea2 = this.bootstrapper.GetInstance(typeof(IEventAggregator));
            Assert.AreEqual(ea1, ea2);
        }

        [Test]
        public void ReturnsCorrectMessageBoxViewModel()
        {
            var mb = this.bootstrapper.GetInstance(typeof(IMessageBoxViewModel));
            Assert.IsInstanceOf<MessageBoxViewModel>(mb);
        }

        [Test]
        public void ReturnsTransientMessageBoxViewModel()
        {
            var mb1 = this.bootstrapper.GetInstance(typeof(IMessageBoxViewModel));
            var mb2 = this.bootstrapper.GetInstance(typeof(IMessageBoxViewModel));
            Assert.AreNotEqual(mb1, mb2);
        }

        [Test]
        public void ResolvesAutoSelfBoundTypesFromCallingAssemblyAsTransient()
        {
            if (!this.Autobinds)
                Assert.Ignore("Autobinding not supported");

            Assert.DoesNotThrow(() => this.bootstrapper.GetInstance(typeof(TestRootViewModel)));
            var vm1 = this.bootstrapper.GetInstance(typeof(TestRootViewModel));
            var vm2 = this.bootstrapper.GetInstance(typeof(TestRootViewModel));

            Assert.NotNull(vm1);
            Assert.AreNotEqual(vm1, vm2); 
        }

        [Test]
        public void ResolvesAutoSelfBoundTypesFromOwnAssemblyAsTransient()
        {
            if (!this.Autobinds)
                Assert.Ignore("Autobinding not supported");

            // Pick a random class with no dependencies...
            Assert.DoesNotThrow(() => this.bootstrapper.GetInstance(typeof(StubType)));
            var vm1 = this.bootstrapper.GetInstance(typeof(StubType));
            var vm2 = this.bootstrapper.GetInstance(typeof(StubType));

            Assert.NotNull(vm1);
            Assert.AreNotEqual(vm1, vm2);
        }

        [Test]
        public void DoesNotMultiplyDisposeWindowManagerConfig()
        {
            // The bootstrapper implements the IWindowManagerConfig. Fetch the IWindowManager to force the
            // IWindowManagerConfig to be constructed, then dispose the bootstrapper, and make sure that
            // the container doesn't dispose the IWindowManagerConfig again

            var windowManager = this.bootstrapper.GetInstance(typeof(IWindowManager));
            this.bootstrapper.Dispose();

            Assert.AreEqual(1, this.bootstrapper.DisposeCount);
        }

        [Test]
        public void DoesNotDisposeTransientInstances()
        {
            StubType.Reset();

            var vm = this.bootstrapper.GetInstance(typeof(StubType));
            this.bootstrapper.Dispose();
            Assert.AreEqual(0, StubType.DisposeCount);

        }
    }
}
