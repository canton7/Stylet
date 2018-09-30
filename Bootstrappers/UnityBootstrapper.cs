using Microsoft.Practices.Unity;
using Stylet;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;

namespace Bootstrappers
{
    public class UnityBootstrapper<TRootViewModel> : BootstrapperBase where TRootViewModel : class
    {
        private IUnityContainer container;

        private object _rootViewModel;
        protected virtual object RootViewModel
        {
            get { return this._rootViewModel ?? (this._rootViewModel = this.GetInstance(typeof(TRootViewModel))); }
        }

        protected override void ConfigureBootstrapper()
        {
            this.container = new UnityContainer();
            this.DefaultConfigureIoC(this.container);
            this.ConfigureIoC(this.container);
        }

        /// <summary>
        /// Carries out default configuration of the IoC container. Override if you don't want to do this
        /// </summary>
        protected virtual void DefaultConfigureIoC(IUnityContainer container)
        {
            var viewManagerConfig = new ViewManagerConfig()
            {
                ViewFactory = this.GetInstance,
                ViewAssemblies = new List<Assembly>() { this.GetType().Assembly }
            };
            var viewManager = new ViewManager(viewManagerConfig);
            // For some reason using ContainerControlledLifetimeManager results in a transient registration....
            container.RegisterInstance<IViewManager>(viewManager);
            container.RegisterInstance<IWindowManager>(new WindowManager(viewManager, () => container.Resolve<IMessageBoxViewModel>(), this));
            container.RegisterInstance<IEventAggregator>(new EventAggregator());
            container.RegisterType<IMessageBoxViewModel, MessageBoxViewModel>(new PerResolveLifetimeManager());
            container.RegisterTypes(AllClasses.FromAssemblies(this.GetType().Assembly), WithMappings.None, WithName.Default, WithLifetime.PerResolve);
        }

        /// <summary>
        /// Override to add your own types to the IoC container.
        /// </summary>
        protected virtual void ConfigureIoC(IUnityContainer container) { }

        public override object GetInstance(Type type)
        {
            return this.container.Resolve(type);
        }

        protected override void Launch()
        {
            base.DisplayRootView(this.RootViewModel);
        }

        public override void Dispose()
        {
            ScreenExtensions.TryDispose(this._rootViewModel);
            if (this.container != null)
                this.container.Dispose();

            base.Dispose();
        }
    }
}
 