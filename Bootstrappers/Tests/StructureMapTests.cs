using NUnit.Framework;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bootstrappers.Tests
{
    public class MyStructureMapBootstrapper : StructureMapBootstrapper<TestRootViewModel>, ITestBootstrapper
    {
        public List<string> ConfigureLog { get; set; }

        public int DisposeCount { get; private set; }

        public MyStructureMapBootstrapper()
        {
            this.ConfigureLog = new List<string>();
        }

        protected override void Configure()
        {
            base.Configure();
            this.ConfigureLog.Add("Configure");
        }

        protected override void DefaultConfigureIoC(ConfigurationExpression config)
        {
            base.DefaultConfigureIoC(config);
            this.ConfigureLog.Add("DefaultConfigureIoC");
        }

        protected override void ConfigureIoC(ConfigurationExpression config)
        {
            base.ConfigureIoC(config);
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

    [TestFixture(Category = "StructureMap")]
    public class StructureMapTests : BootstrapperTests<MyStructureMapBootstrapper>
    {
        public StructureMapTests()
        {
            this.Autobinds = true;
        }

        public override MyStructureMapBootstrapper CreateBootstrapper()
        {
            return new MyStructureMapBootstrapper();
        }
    }
}
