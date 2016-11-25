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

        public int DisposeCount { get; private set; }

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

        public new object GetInstance(Type type)
        {
            return base.GetInstance(type);
        }

        public new void ConfigureBootstrapper()
        {
            base.ConfigureBootstrapper();
        }

        public override void Dispose()
        {
            base.Dispose();
            this.DisposeCount++;
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
