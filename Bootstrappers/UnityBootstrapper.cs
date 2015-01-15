using Microsoft.Practices.Unity;
using Stylet;
using System;
using System.Windows;

namespace Bootstrappers
{
    public class UnityBootstrapper<TRootViewModel> : BootstrapperBase where TRootViewModel : class
    {
        private IUnityContainer container;

        private object _rootViewModel;
        protected override object RootViewModel
        {
            get { return this._rootViewModel ?? (this._rootViewModel = this.GetInstance(typeof(TRootViewModel))); }
        }

        protected override void ConfigureBootstrapper()
        {
            this.Configure();

            this.container = new UnityContainer();
            this.DefaultConfigureIoC(this.container);
            this.ConfigureIoC(this.container);
        }

        /// <summary>
        /// Override to configure anything that needs configuring
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

        public override object GetInstance(Type type)
        {
            return this.container.Resolve(type);
        }

        protected override void OnExitInternal(ExitEventArgs e)
        {
            this.container.Dispose();
        }
    }
}
 