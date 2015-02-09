using StyletIoC;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Stylet
{
    /// <summary>
    /// Bootstrapper to be extended by any application which wants to use StyletIoC (the default)
    /// </summary>
    /// <typeparam name="TRootViewModel">Type of the root ViewModel. This will be instantiated and displayed</typeparam>
    public abstract class Bootstrapper<TRootViewModel> : BootstrapperBase where TRootViewModel : class
    {
        /// <summary>
        /// Gets or sets the Bootstrapper's IoC container. This is created after ConfigureIoC has been run.
        /// </summary>
        protected IContainer Container { get; set; }

        private object _rootViewModel;

        /// <summary>
        /// Gets the instance of the root ViewMode, which is displayed at launch
        /// </summary>
        protected override object RootViewModel
        {
            get { return this._rootViewModel ?? (this._rootViewModel = this.GetInstance(typeof(TRootViewModel))); }
        }

        /// <summary>
        /// Overridden from BootstrapperBase, this sets up the IoC container
        /// </summary>
        protected override sealed void ConfigureBootstrapper()
        {
            var builder = new StyletIoCBuilder();

            this.DefaultConfigureIoC(builder);
            this.ConfigureIoC(builder);

            this.Container = builder.BuildContainer();

            this.Configure();
        }

        /// <summary>
        /// Hook called after the IoC container has been set up
        /// </summary>
        protected virtual void Configure() { }

        /// <summary>
        /// Carries out default configuration of StyletIoC. Override if you don't want to do this
        /// </summary>
        /// <param name="builder">StyletIoC builder to use to configure the container</param>
        protected virtual void DefaultConfigureIoC(StyletIoCBuilder builder)
        {
            // Mark these as auto-bindings, so the user can replace them if they want
            builder.Bind<IViewManagerConfig>().ToInstance(this).AsWeakBinding();
            builder.Bind<IViewManager>().To<ViewManager>().InSingletonScope().AsWeakBinding();
            builder.Bind<IWindowManager>().To<WindowManager>().InSingletonScope().AsWeakBinding();
            builder.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope().AsWeakBinding();
            builder.Bind<IMessageBoxViewModel>().To<MessageBoxViewModel>().AsWeakBinding();

            builder.Autobind(this.Assemblies);
        }

        /// <summary>
        /// Override to add your own types to the IoC container.
        /// </summary>
        /// <param name="builder">StyletIoC builder to use to configure the container</param>
        protected virtual void ConfigureIoC(IStyletIoCBuilder builder) { }

        /// <summary>
        /// Given a type, use the IoC container to fetch an instance of it
        /// </summary>
        /// <param name="type">Type to fetch</param>
        /// <returns>Fetched instance</returns>
        public override object GetInstance(Type type)
        {
            return this.Container.Get(type);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            this.Container.Dispose();
            base.Dispose();
        }
    }
}
