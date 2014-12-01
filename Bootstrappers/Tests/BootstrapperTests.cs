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
        T GetInstance<T>();
        void ConfigureBootstrapper();
        List<string> ConfigureLog { get; }
    }

    public abstract class BootstrapperTests<TBootstrapper> where TBootstrapper : ITestBootstrapper
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
            Assert.That(this.bootstrapper.ConfigureLog, Is.EquivalentTo(new[] { "Configure", "DefaultConfigureIoC", "ConfigureIoC" }));
        }

        [Test]
        public void ReturnsCorrectViewManager()
        {
            var vm = this.bootstrapper.GetInstance<IViewManager>();
            Assert.IsInstanceOf<ViewManager>(vm);
        }

        [Test]
        public void ReturnsSingletonViewManager()
        {
            var vm1 = this.bootstrapper.GetInstance<IViewManager>();
            var vm2 = this.bootstrapper.GetInstance<IViewManager>();
            Assert.AreEqual(vm1, vm2);
        }

        [Test]
        public void ReturnsCorrectWindowManager()
        {
            var wm = this.bootstrapper.GetInstance<IWindowManager>();
            Assert.IsInstanceOf<WindowManager>(wm);
        }

        [Test]
        public void ReturnsSingletonWindowManager()
        {
            var wm1 = this.bootstrapper.GetInstance<IWindowManager>();
            var wm2 = this.bootstrapper.GetInstance<IWindowManager>();
            Assert.AreEqual(wm1, wm2);
        }

        [Test]
        public void ReturnsCorrectEventAggregator()
        {
            var ea = this.bootstrapper.GetInstance<IEventAggregator>();
            Assert.IsInstanceOf<EventAggregator>(ea);
        }

        [Test]
        public void ReturnsSingletonEventAggregator()
        {
            var ea1 = this.bootstrapper.GetInstance<IEventAggregator>();
            var ea2 = this.bootstrapper.GetInstance<IEventAggregator>();
            Assert.AreEqual(ea1, ea2);
        }

        [Test]
        public void ReturnsCorrectMessageBoxViewModel()
        {
            var mb = this.bootstrapper.GetInstance<IMessageBoxViewModel>();
            Assert.IsInstanceOf<MessageBoxViewModel>(mb);
        }

        [Test]
        public void ReturnsTransientMessageBoxViewModel()
        {
            var mb1 = this.bootstrapper.GetInstance<IMessageBoxViewModel>();
            var mb2 = this.bootstrapper.GetInstance<IMessageBoxViewModel>();
            Assert.AreNotEqual(mb1, mb2);
        }

        [Test]
        public void ResolvesAutoSelfBoundTypesFromCallingAssemblyAsTransient()
        {
            if (!this.Autobinds)
                Assert.Ignore("Autobinding not supported");

            Assert.DoesNotThrow(() => this.bootstrapper.GetInstance<TestRootViewModel>());
            var vm1 = this.bootstrapper.GetInstance<TestRootViewModel>();
            var vm2 = this.bootstrapper.GetInstance<TestRootViewModel>();

            Assert.NotNull(vm1);
            Assert.AreNotEqual(vm1, vm2); 
        }

        [Test]
        public void ResolvesAutoSelfBoundTypesFromOwnAssemblyAsTransient()
        {
            if (!this.Autobinds)
                Assert.Ignore("Autobinding not supported");

            // Pick a random class with no dependencies...
            Assert.DoesNotThrow(() => this.bootstrapper.GetInstance<StyletIoC.StyletIoCBuilder>());
            var vm1 = this.bootstrapper.GetInstance<StyletIoC.StyletIoCBuilder>();
            var vm2 = this.bootstrapper.GetInstance<StyletIoC.StyletIoCBuilder>();

            Assert.NotNull(vm1);
            Assert.AreNotEqual(vm1, vm2);
        }
    }
}
