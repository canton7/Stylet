using Moq;
using NUnit.Framework;
using Stylet;
using StyletIoC;
using System;

namespace StyletUnitTests
{
    [TestFixture]
    public class BootstrapperTests
    {
        private interface I1 { }
        private class C1 : I1 { }

        private class RootViewModel : IDisposable
        {
            public bool Disposed;

            public void Dispose()
            {
                this.Disposed = true;
            }
        }

        private class RandomClass { }

        private class MyBootstrapper<T> : Bootstrapper<T> where T : class
        {
            public new IContainer Container
            {
                get { return base.Container; }
                set { base.Container = value; }
            }

            public new T RootViewModel
            {
                get { return base.RootViewModel; }
            }

            public new void Configure()
            {
                base.ConfigureBootstrapper();
            }

            public RootViewModel MyRootViewModel = new RootViewModel();

            public bool ConfigureIoCCalled;
            protected override void ConfigureIoC(IStyletIoCBuilder builder)
            {
                this.ConfigureIoCCalled = true;
                builder.Bind<I1>().To<C1>();
                // Singleton, so we can test against it
                builder.Bind<RootViewModel>().ToInstance(this.MyRootViewModel).DisposeWithContainer(false);
                base.ConfigureIoC(builder);
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
            Assert.IsInstanceOf<ViewManager>(ioc.Get<ViewManager>());
            Assert.IsInstanceOf<MessageBoxViewModel>(ioc.Get<IMessageBoxViewModel>());

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
        public void DisposeDisposesContainer()
        {
            var container = new Mock<IContainer>();
            this.bootstrapper.Container = container.Object;
            this.bootstrapper.Dispose();
            container.Verify(x => x.Dispose());
        }

        [Test]
        public void DisposeDoesNotCreateRootViewModel()
        {
            this.bootstrapper.Configure();
            this.bootstrapper.Dispose();
            Assert.False(this.bootstrapper.MyRootViewModel.Disposed);
        }

        [Test]
        public void DisposeDisposesRootViewModel()
        {
            this.bootstrapper.Configure();

            // Force it to be created
            var dummy = this.bootstrapper.RootViewModel;
            this.bootstrapper.Dispose();
            Assert.True(this.bootstrapper.MyRootViewModel.Disposed);
        }
    }
}
