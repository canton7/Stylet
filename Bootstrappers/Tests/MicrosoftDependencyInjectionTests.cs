using Autofac;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bootstrappers.Tests
{
    public class MyMicrosoftDependencyInjectionBootstrapper : MicrosoftDependencyInjectionBootstrapper<TestRootViewModel>, ITestBootstrapper
    {
        public List<string> ConfigureLog { get; set; }

        public int DisposeCount { get; private set; }

        public MyMicrosoftDependencyInjectionBootstrapper()
        {
            this.ConfigureLog = new List<string>();
        }

        protected override void Configure()
        {
            base.Configure();
            this.ConfigureLog.Add("Configure");
        }

        protected override void DefaultConfigureIoC(IServiceCollection services)
        {
            base.DefaultConfigureIoC(services);
            this.ConfigureLog.Add("DefaultConfigureIoC");
        }

        protected override void ConfigureIoC(IServiceCollection services)
        {
            base.ConfigureIoC(services);
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

    [TestFixture(Category = "ServiceCollection")]
    public class MicrosoftDependencyInjectionTests : BootstrapperTests<MyMicrosoftDependencyInjectionBootstrapper>
    {
        public MicrosoftDependencyInjectionTests()
        {
            this.Autobinds = false;
        }

        public override MyMicrosoftDependencyInjectionBootstrapper CreateBootstrapper()
        {
            return new MyMicrosoftDependencyInjectionBootstrapper();
        }
    }
}
