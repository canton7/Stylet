using StyletIoC;
using System;
using System.Collections.Generic;

namespace Stylet
{
    /// <summary>
    /// Bootstrapper to be extended by any application which wants to use StyletIoC (the default)
    /// </summary>
    /// <typeparam name="TRootViewModel">Type of the root ViewModel. This will be instantiated and displayed</typeparam>
    public class Bootstrapper<TRootViewModel> : BootstrapperBase<TRootViewModel> where TRootViewModel : class
    {
        /// <summary>
        /// Create a new Bootstrapper
        /// </summary>
        public Bootstrapper() : base() { }

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
            builder.BindWeak(typeof(IWindowManager)).To<WindowManager>().InSingletonScope();
            builder.BindWeak(typeof(IEventAggregator)).To<EventAggregator>().InSingletonScope();
            builder.BindWeak(typeof(IViewManager)).To<ViewManager>().InSingletonScope();
            builder.BindWeak(typeof(IMessageBoxViewModel)).To<MessageBoxViewModel>();

            builder.Autobind(AssemblySource.Assemblies);
        }

        /// <summary>
        /// Override to add your own types to the IoC container.
        /// </summary>
        /// <param name="builder">StyletIoC builder to use to configure the container</param>
        protected virtual void ConfigureIoC(IStyletIoCBuilder builder) { }

        /// <summary>
        /// Override which uses StyletIoC as the implementation for IoC.Get
        /// </summary>
        protected override object GetInstance(Type service, string key = null)
        {
            return this.Container.Get(service, key);
        }

        /// <summary>
        /// Override which uses StyletIoC as the implementation for IoC.GetAll
        /// </summary>
        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return this.Container.GetAll(service);
        }

        /// <summary>
        /// Override which uses StyletIoC as the implementation for IoC.BuildUp
        /// </summary>
        protected override void BuildUp(object instance)
        {
            this.Container.BuildUp(instance);
        }
    }
}
