using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bootstrappers.Tests
{
    public class MyNoIocContainerBootstrapper : NoIoCContainerBootstrapper<TestRootViewModel>, ITestBootstrapper
    {
        public List<string> ConfigureLog { get; set; }

        public MyNoIocContainerBootstrapper()
        {
            this.ConfigureLog = new List<string>();
        }

        protected override void Configure()
        {
            base.Configure();
            this.ConfigureLog.Add("Configure");
        }

        protected override void DefaultConfigureContainer()
        {
            base.DefaultConfigureContainer();
            this.ConfigureLog.Add("DefaultConfigureIoC");
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            this.ConfigureLog.Add("ConfigureIoC");
        }

        public new object GetInstance(Type type)
        {
            return base.GetInstance(type);
        }

        public new void ConfigureBootstrapper()
        {
            base.ConfigureBootstrapper();
        }
    }

    [TestFixture(Category = "NoIoCContainer")]
    public class NoIoCContainerTests : BootstrapperTests<MyNoIocContainerBootstrapper>
    {
        public NoIoCContainerTests()
        {
            this.Autobinds = false;
        }

        public override MyNoIocContainerBootstrapper CreateBootstrapper()
        {
            return new MyNoIocContainerBootstrapper();
        }
    }
}
