using StyletIoC;
using System;

namespace Stylet.Samples.OverridingViewManager
{
    public class Bootstrapper : Bootstrapper<ShellViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            builder.Bind<IViewManager>().To<CustomViewManager>();
        }
    }
}
