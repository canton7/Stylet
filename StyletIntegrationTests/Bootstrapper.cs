using Stylet;
using StyletIntegrationTests.BootstrapperIoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyletIntegrationTests
{
    public class Bootstrapper : Bootstrapper<ShellViewModel>
    {
        protected override void ConfigureIoC(StyletIoC.IStyletIoCBuilder builder)
        {
            builder.Bind<BootstrapperIoCI1>().ToAllImplementations();
        }
    }
}
