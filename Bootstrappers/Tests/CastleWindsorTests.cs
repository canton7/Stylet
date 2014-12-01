using Castle.Windsor;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bootstrappers.Tests
{
    public class MyCastleWindsorBootstrapper : CastleWindsorBootstrapper<TestRootViewModel>, ITestBootstrapper
    {
        public List<string> ConfigureLog { get; set; }

        public MyCastleWindsorBootstrapper()
        {
            this.ConfigureLog = new List<string>();
        }

        protected override void Configure()
        {
            base.Configure();
            this.ConfigureLog.Add("Configure");
        }

        protected override void DefaultConfigureIoC(IWindsorContainer container)
        {
            base.DefaultConfigureIoC(container);
            this.ConfigureLog.Add("DefaultConfigureIoC");
        }

        protected override void ConfigureIoC(IWindsorContainer container)
        {
            base.ConfigureIoC(container);
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

    [TestFixture(Category = "Castle Windsor")]
    public class CastleWindsorTests : BootstrapperTests<MyCastleWindsorBootstrapper>
    {
        public CastleWindsorTests()
        {
            this.Autobinds = true;
        }

        public override MyCastleWindsorBootstrapper CreateBootstrapper()
        {
            return new MyCastleWindsorBootstrapper();
        }
    }
}
