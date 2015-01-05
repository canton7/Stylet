using Moq;
using NUnit.Framework;
using Stylet;
using Stylet.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StyletUnitTests
{
    [TestFixture, RequiresSTA]
    public class BootstrapperBaseTests
    {
        private class RootViewModel { }

        private class MyBootstrapperBase<T> : BootstrapperBase<T> where T : class
        {
            private IViewManager viewManager;
            private IWindowManager windowManager;

            public MyBootstrapperBase(IViewManager viewManager, IWindowManager windowManager)
            {
                this.viewManager = viewManager;
                this.windowManager = windowManager;

                this.Start(new string[0]);
            }

            public new Application Application
            {
                get { return base.Application; }
            }

            public bool GetInstanceCalled;
            public override object GetInstance(Type service)
            {
                this.GetInstanceCalled = true;
                if (service == typeof(IViewManager))
                    return this.viewManager;
                if (service == typeof(IWindowManager))
                    return this.windowManager;
                if (service == typeof(RootViewModel))
                    return new RootViewModel();
                return null;
            }

            public bool OnStartupCalled;
            protected override void OnStartup()
            {
                this.OnStartupCalled = true;
            }

            public bool OnExitCalled;
            protected override void OnExit(ExitEventArgs e)
            {
                this.OnExitCalled = true;
            }

            public bool ConfigureCalled;
            protected override void ConfigureBootstrapper()
            {
                this.ConfigureCalled = true;
                base.ConfigureBootstrapper();
            }

            public new void Start(string[] args)
            {
                base.Start(args);
            }
        }

        private class FakeDispatcher : IDispatcher
        {
            public void Post(Action action)
            {
                throw new NotImplementedException();
            }

            public void Send(Action action)
            {
                throw new NotImplementedException();
            }

            public bool IsCurrent
            {
                get { throw new NotImplementedException(); }
            }
        }

        
        private MyBootstrapperBase<RootViewModel> bootstrapper;
        private Mock<IViewManager> viewManager;
        private Mock<IWindowManager> windowManager;

        private IDispatcher dispatcher;

        [SetUp]
        public void SetUp()
        {
            this.dispatcher = Execute.Dispatcher;
            this.viewManager = new Mock<IViewManager>();
            this.windowManager = new Mock<IWindowManager>();
            this.bootstrapper = new MyBootstrapperBase<RootViewModel>(this.viewManager.Object, this.windowManager.Object);
        }

        [TearDown]
        public void TearDown()
        {
            Execute.Dispatcher = this.dispatcher;
        }

        [Test]
        public void SetupThrowsIfApplicationIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => this.bootstrapper.Setup(null));
        }

        [Test]
        public void StartCallsConfigure()
        {
            this.bootstrapper.Start(new string[0]);
            Assert.True(this.bootstrapper.ConfigureCalled);
        }

        [Test]
        public void StartAssignsViewManager()
        {
            this.bootstrapper.Start(new string[0]);
            Assert.AreEqual(View.ViewManager, this.viewManager.Object);
        }

        [Test]
        public void StartAssignsArgs()
        {
            this.bootstrapper.Start(new[] { "one", "two" });
            Assert.That(this.bootstrapper.Args, Is.EquivalentTo(new[] { "one", "two" }));
        }

        [Test]
        public void StartCallsOnStartup()
        {
            this.bootstrapper.Start(new string[0]);
            Assert.True(this.bootstrapper.OnStartupCalled);
        }
    }
}
