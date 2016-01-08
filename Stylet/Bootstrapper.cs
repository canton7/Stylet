using StyletIoC;
using System;
using System.Collections.Generic;
using System.Reflection;

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
            builder.Assemblies = new List<Assembly>(new List<Assembly>() { this.GetType().Assembly });

            // Call DefaultConfigureIoC *after* ConfigureIoIC, so that they can customize builder.Assemblies
            this.ConfigureIoC(builder);
            this.DefaultConfigureIoC(builder);

            this.Container = builder.BuildContainer();
        }

        /// <summary>
        /// Carries out default configuration of StyletIoC. Override if you don't want to do this
        /// </summary>
        /// <param name="builder">StyletIoC builder to use to configure the container</param>
        protected virtual void DefaultConfigureIoC(StyletIoCBuilder builder)
        {
            // Mark these as weak-bindings, so the user can replace them if they want

            var viewManager = new ViewManager(this.GetInstance, new List<Assembly>() { this.GetType().Assembly });
            // Bind it to both IViewManager and to itself, so that people can get it with Container.Get<ViewManager>()
            builder.Bind<IViewManager>().And<ViewManager>().ToInstance(viewManager).AsWeakBinding();

            builder.Bind<IWindowManagerConfig>().ToInstance(this).AsWeakBinding();
            builder.Bind<IWindowManager>().To<WindowManager>().InSingletonScope().AsWeakBinding();
            builder.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope().AsWeakBinding();
            builder.Bind<IMessageBoxViewModel>().To<MessageBoxViewModel>().AsWeakBinding();

            builder.Autobind();
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
            // Dispose the container last
            base.Dispose();
            this.Container.Dispose();
        }
    }
}
