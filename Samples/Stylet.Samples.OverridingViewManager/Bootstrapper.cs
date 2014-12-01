using StyletIoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stylet.Samples.OverridingViewManager
{
    public class Bootstrapper : Bootstrapper<ShellViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            base.ConfigureIoC(builder);

            builder.Bind<IViewManager>().ToInstance(new CustomViewManager(this.Assemblies, type => this.Container.Get(type)));
        }
    }
}
