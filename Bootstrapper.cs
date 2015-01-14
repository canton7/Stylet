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
    public abstract class Bootstrapper<TRootViewModel> : BootstrapperBase<TRootViewModel> where TRootViewModel : class
    {
        /// <summary>
        /// Gets or sets the Bootstrapper's IoC container. This is created after ConfigureIoC has been run.
        /// </summary>
        protected IContainer Container { get; set; }

        /// <summary>
        /// Overridden from BootstrapperBase, this sets up the IoC container
        /// </summary>
        protected override sealed void ConfigureBootstrapper()
        {
            // This needs to be called before the container is set up, as it might affect the assemblies
            this.Configure();

            var builder = new StyletIoCBuilder();

            this.DefaultConfigureIoC(builder);
            this.ConfigureIoC(builder);

            this.Container = builder.BuildContainer();
        }

        /// <summary>
        /// Override to configure your IoC container, and anything else
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
        /// Hook used internall by the Bootstrapper to do things like dispose the IoC container
        /// </summary>
        /// <param name="e">The exit event data</param>
        protected override void OnExitInternal(ExitEventArgs e)
        {
            base.OnExitInternal(e);
            this.Container.Dispose();
        }
    }
}
