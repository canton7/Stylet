using Autofac;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bootstrappers.Tests
{
    public class MyAutofacBootstrapper : AutofacBootstrapper<TestRootViewModel>, ITestBootstrapper
    {
        public List<string> ConfigureLog { get; set; }

        public MyAutofacBootstrapper()
        {
            this.ConfigureLog = new List<string>();
        }

        protected override void Configure()
        {
            base.Configure();
            this.ConfigureLog.Add("Configure");
        }

        protected override void DefaultConfigureIoC(ContainerBuilder builder)
        {
            base.DefaultConfigureIoC(builder);
            this.ConfigureLog.Add("DefaultConfigureIoC");
        }

        protected override void ConfigureIoC(ContainerBuilder builder)
        {
            base.ConfigureIoC(builder);
            this.ConfigureLog.Add("ConfigureIoC");
        }

        public new T GetInstance<T>()
        {
            return base.GetInstance<T>();
        }

        public new void ConfigureBootstrapper()
        {
            base.ConfigureBootstrapper();
        }
    }

    [TestFixture(Category = "Autofac")]
    public class AutofacTests : BootstrapperTests<MyAutofacBootstrapper>
    {
        public AutofacTests()
        {
            this.Autobinds = true;
        }

        public override MyAutofacBootstrapper CreateBootstrapper()
        {
            return new MyAutofacBootstrapper();
        }
    }
}
