using Ninject;
using Stylet;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;

namespace Bootstrappers
{
    public class NinjectBootstrapper<TRootViewModel> : BootstrapperBase where TRootViewModel : class
    {
        private IKernel kernel;

        private object _rootViewModel;
        protected virtual object RootViewModel
        {
            get { return this._rootViewModel ?? (this._rootViewModel = this.GetInstance(typeof(TRootViewModel))); }
        }

        protected override void ConfigureBootstrapper()
        {
            this.kernel = new StandardKernel();
            this.DefaultConfigureIoC(this.kernel);
            this.ConfigureIoC(this.kernel);
        }

        /// <summary>
        /// Carries out default configuration of the IoC container. Override if you don't want to do this
        /// </summary>
        protected virtual void DefaultConfigureIoC(IKernel kernel)
        {
            var viewManagerConfig = new ViewManagerConfig()
            {
                ViewFactory = this.GetInstance,
                ViewAssemblies = new List<Assembly>() { this.GetType().Assembly }
            };
            kernel.Bind<IViewManager>().ToConstant(new ViewManager(viewManagerConfig));

            kernel.Bind<IWindowManagerConfig>().ToConstant(this).InTransientScope();
            kernel.Bind<IWindowManager>().ToMethod(c => new WindowManager(c.Kernel.Get<IViewManager>(), () => c.Kernel.Get<IMessageBoxViewModel>(), c.Kernel.Get<IWindowManagerConfig>())).InSingletonScope();
            kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
            kernel.Bind<IMessageBoxViewModel>().To<MessageBoxViewModel>(); // Not singleton!
        }

        /// <summary>
        /// Override to add your own types to the IoC container.
        /// </summary>
        protected virtual void ConfigureIoC(IKernel kernel) { }

        public override object GetInstance(Type type)
        {
            return this.kernel.Get(type);
        }

        protected override void Launch()
        {
            base.DisplayRootView(this.RootViewModel);
        }

        public override void Dispose()
        {
            ScreenExtensions.TryDispose(this._rootViewModel);
            if (this.kernel != null)
                this.kernel.Dispose();

            base.Dispose();
        }
    }
}
