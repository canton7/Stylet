using Ninject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bootstrappers.Tests
{
    public class MyNinjectBootstrapper : NinjectBootstrapper<TestRootViewModel>, ITestBootstrapper
    {
        public List<string> ConfigureLog { get; set; }

        public MyNinjectBootstrapper()
        {
            this.ConfigureLog = new List<string>();
        }

        protected override void Configure()
        {
            base.Configure();
            this.ConfigureLog.Add("Configure");
        }

        protected override void DefaultConfigureIoC(IKernel kernel)
        {
            base.DefaultConfigureIoC(kernel);
            this.ConfigureLog.Add("DefaultConfigureIoC");
        }

        protected override void ConfigureIoC(IKernel kernel)
        {
            base.ConfigureIoC(kernel);
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

    [TestFixture(Category="Ninject")]
    public class NinjectTests : BootstrapperTests<MyNinjectBootstrapper>
    {
        public NinjectTests()
        {
            this.Autobinds = true;
        }

        public override MyNinjectBootstrapper CreateBootstrapper()
        {
            return new MyNinjectBootstrapper();
        }
    }
}
