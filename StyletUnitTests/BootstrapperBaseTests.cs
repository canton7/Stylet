using Moq;
using NUnit.Framework;
using Stylet;
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
            }

            public bool GetInstanceCalled;
            protected override object GetInstance(Type service, string key = null)
            {
                this.GetInstanceCalled = true;
                if (service == typeof(IViewManager))
                    return this.viewManager;
                if (service == typeof(IWindowManager))
                    return this.windowManager;
                if (service == typeof(RootViewModel))
                    return new RootViewModel();
                return new object();
            }

            public bool GetAllInstancesCalled;
            protected override IEnumerable<object> GetAllInstances(Type service)
            {
                this.GetAllInstancesCalled = true;
                return Enumerable.Empty<object>();
            }

            public bool BuildUpCalled;
            protected override void BuildUp(object instance)
            {
                this.BuildUpCalled = true;
            }

            public bool OnExitCalled;
            protected override void OnExit(object sender, ExitEventArgs e)
            {
                this.OnExitCalled = true;
            }

            public bool ConfigureResourcesCalled;
            protected override void ConfigureResources()
            {
                this.ConfigureResourcesCalled = true;
                base.ConfigureResources();
            }

            public bool ConfigureCalled;
            protected override void Configure()
            {
                this.ConfigureCalled = true;
                base.Configure();
            }

            public new void Start()
            {
                base.Start();
            }
        }

        
        private MyBootstrapperBase<RootViewModel> bootstrapper;
        private Mock<IViewManager> viewManager;
        private Mock<IWindowManager> windowManager;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Execute.TestExecuteSynchronously = true;
            AssemblySource.Assemblies.Clear();
        }

        [SetUp]
        public void SetUp()
        {
            this.viewManager = new Mock<IViewManager>();
            this.windowManager = new Mock<IWindowManager>();
            this.bootstrapper = new MyBootstrapperBase<RootViewModel>(this.viewManager.Object, this.windowManager.Object);
        }

        [Test]
        public void SetsUpOnExitHandler()
        {
            var ctor = typeof(ExitEventArgs).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0];
            var ea = (ExitEventArgs)ctor.Invoke(new object[] { 3 });
            //this.application.OnExit(ea);
            //Assert.True(this.bootstrapper.OnExitCalled);
        }

        [Test]
        public void AssignsIoCGetInstanceToGetInstance()
        {
            IoC.GetInstance(typeof(string), null);
            Assert.True(this.bootstrapper.GetInstanceCalled);
        }

        [Test]
        public void AssignsIoCGetAllInstancesToGetAllInstances()
        {
            IoC.GetAllInstances(typeof(string));
            Assert.True(this.bootstrapper.GetAllInstancesCalled);
        }

        [Test]
        public void AssignsIoCBuildUpToBuildUp()
        {
            IoC.BuildUp(new object());
            Assert.True(this.bootstrapper.BuildUpCalled);
        }

        [Test]
        public void StartAssignsExecuteDispatcher()
        {
            Execute.Dispatcher = null;
            this.bootstrapper.Start();
            Assert.NotNull(Execute.Dispatcher); // Can't test any further, unfortunately
        }

        [Test]
        public void StartSetsUpAssemblySource()
        {
            AssemblySource.Assemblies.Add(null);
            this.bootstrapper.Start();
            Assert.That(AssemblySource.Assemblies, Is.EquivalentTo(new[] { this.bootstrapper.GetType().Assembly }));
        }

        [Test]
        public void StartCallsConfigureResources()
        {
            this.bootstrapper.Start();
            Assert.True(this.bootstrapper.ConfigureResourcesCalled);
        }

        [Test]
        public void StartCallsConfigure()
        {
            this.bootstrapper.Start();
            Assert.True(this.bootstrapper.ConfigureCalled);
        }
    }
}
