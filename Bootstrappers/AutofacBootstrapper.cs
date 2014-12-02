using Autofac;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bootstrappers
{
    public class AutofacBootstrapper<TRootViewModel> : BootstrapperBase<TRootViewModel> where TRootViewModel : class
    {
        private IContainer container;

        protected override void ConfigureBootstrapper()
        {
            this.Configure();

            var builder = new ContainerBuilder();
            this.DefaultConfigureIoC(builder);
            this.ConfigureIoC(builder);
            this.container = builder.Build();
        }

        /// <summary>
        /// Override to configure your IoC container, and anything else
        /// </summary>
        protected virtual void Configure() { }

        /// <summary>
        /// Carries out default configuration of the IoC container. Override if you don't want to do this
        /// </summary>
        protected virtual void DefaultConfigureIoC(ContainerBuilder builder)
        {
            builder.RegisterInstance<IViewManagerConfig>(this);
            builder.RegisterType<ViewManager>().As<IViewManager>().SingleInstance();
            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
            builder.RegisterType<MessageBoxViewModel>().As<IMessageBoxViewModel>(); // Not singleton!
            builder.RegisterAssemblyTypes(this.Assemblies.ToArray());
        }

        /// <summary>
        /// Override to add your own types to the IoC container.
        /// </summary>
        protected virtual void ConfigureIoC(ContainerBuilder builder) { }

        public override object GetInstance(Type type)
        {
            return this.container.Resolve(type);
        }
    }
}
