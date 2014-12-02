using Microsoft.Practices.Unity;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bootstrappers
{
    public class UnityBootstrapper<TRootViewModel> : BootstrapperBase<TRootViewModel> where TRootViewModel : class
    {
        private IUnityContainer container;

        protected override void ConfigureBootstrapper()
        {
            this.Configure();

            this.container = new UnityContainer();
            this.DefaultConfigureIoC(this.container);
            this.ConfigureIoC(this.container);
        }

        /// <summary>
        /// Override to configure your IoC container, and anything else
        /// </summary>
        protected virtual void Configure() { }

        /// <summary>
        /// Carries out default configuration of the IoC container. Override if you don't want to do this
        /// </summary>
        protected virtual void DefaultConfigureIoC(IUnityContainer container)
        {
            // For some reason using ContainerControlledLifetimeManager results in a transient registration....
            // This is a workaround
            var viewManager = new ViewManager(this);
            container.RegisterInstance<IViewManager>(viewManager);
            container.RegisterInstance<IWindowManager>(new WindowManager(viewManager, () => container.Resolve<IMessageBoxViewModel>()));
            container.RegisterInstance<IEventAggregator>(new EventAggregator());
            container.RegisterType<IMessageBoxViewModel, MessageBoxViewModel>(new PerResolveLifetimeManager());
            container.RegisterTypes(AllClasses.FromAssemblies(this.Assemblies), WithMappings.None, WithName.Default, WithLifetime.PerResolve);
        }

        /// <summary>
        /// Override to add your own types to the IoC container.
        /// </summary>
        protected virtual void ConfigureIoC(IUnityContainer container) { }

        protected override object GetInstance(Type type)
        {
            return this.container.Resolve(type);
        }
    }
}
 