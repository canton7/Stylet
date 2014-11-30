using Microsoft.Practices.Unity;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bootstrappers.Tests
{
    public class MyUnityBootstrapper : UnityBootstrapper<TestRootViewModel>, ITestBootstrapper
    {
        public List<string> ConfigureLog { get; set; }

        public MyUnityBootstrapper()
        {
            this.ConfigureLog = new List<string>();
        }

        protected override void Configure()
        {
            base.Configure();
            this.ConfigureLog.Add("Configure");
        }

        protected override void DefaultConfigureIoC(IUnityContainer container)
        {
            base.DefaultConfigureIoC(container);
            this.ConfigureLog.Add("DefaultConfigureIoC");
        }

        protected override void ConfigureIoC(IUnityContainer container)
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

    [TestFixture(Category = "Unity")]
    public class UnityTests : BootstrapperTests<MyUnityBootstrapper>
    {
        public UnityTests()
        {
            this.Autobinds = true;
        }

        public override MyUnityBootstrapper CreateBootstrapper()
        {
            return new MyUnityBootstrapper();
        }
    }
}
