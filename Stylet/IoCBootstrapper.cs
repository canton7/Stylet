using StyletIoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Stylet
{
    public class IoCBootstrapper<TRootViewModel> : Bootstrapper<TRootViewModel>
    {
        protected IContainer container;

        protected override void Start()
        {
            base.Start();

            var builder = new StyletIoCBuilder();
            this.ConfigureIoC(builder);
            this.container = builder.BuildContainer();
        }

        protected virtual void ConfigureIoC(IStyletIoCBuilder builder)
        {
            builder.Autobind(AssemblySource.Assemblies);
            builder.Bind<IWindowManager>().To<WindowManager>().InSingletonScope();
        }

        protected override object GetInstance(Type service, string key = null)
        {
            return this.container.Get(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return this.container.GetAll(service);
        }

        protected override void BuildUp(object instance)
        {
            this.container.BuildUp(instance);
        }
    }
}
