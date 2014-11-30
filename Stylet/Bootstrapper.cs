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
    public class Bootstrapper<TRootViewModel> : BootstrapperBase<TRootViewModel> where TRootViewModel : class
    {
        /// <summary>
        /// IoC container. This is created after ConfigureIoC has been run.
        /// </summary>
        protected IContainer Container;

        /// <summary>
        /// Overridden from BootstrapperBase, this sets up the IoC container
        /// </summary>
        protected override void Configure()
        {
            base.Configure();

            var builder = new StyletIoCBuilder();

            this.DefaultConfigureIoC(builder);
            this.ConfigureIoC(builder);

            this.Container = builder.BuildContainer();
        }

        /// <summary>
        /// Carries out default configuration of StyletIoC. Override if you don't want to do this
        /// </summary>
        /// <param name="builder">StyletIoC builder to use to configure the container</param>
        protected virtual void DefaultConfigureIoC(StyletIoCBuilder builder)
        {
            // Mark these as auto-bindings, so the user can replace them if they want
            builder.Bind<IViewManager>().ToInstance(new ViewManager(type => this.Container.Get(type) as UIElement)).AsWeakBinding();
            builder.Bind<IWindowManager>().To<WindowManager>().InSingletonScope().AsWeakBinding();
            builder.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope().AsWeakBinding();
            builder.Bind<IMessageBoxViewModel>().To<MessageBoxViewModel>().AsWeakBinding();

            builder.Autobind(AssemblySource.Assemblies);
        }

        /// <summary>
        /// Override to add your own types to the IoC container.
        /// </summary>
        /// <param name="builder">StyletIoC builder to use to configure the container</param>
        protected virtual void ConfigureIoC(IStyletIoCBuilder builder) { }

        /// <summary>
        /// Given a type, use the IoC container to fetch an instance of it
        /// </summary>
        /// <typeparam name="T">Instance of type to fetch</typeparam>
        /// <returns>Fetched instance</returns>
        protected override T GetInstance<T>()
        {
            return this.Container.Get<T>();
        }
    }
}
