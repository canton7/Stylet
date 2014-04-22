using StyletIoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
        protected IContainer container;

        /// <summary>
        /// Called from the constructor, this does everything necessary to start the application, including set up StyletIoC
        /// </summary>
        protected override void Start()
        {
            base.Start();

            var builder = new StyletIoCBuilder();

            this.DefaultConfigureIoC(builder);
            this.ConfigureIoC(builder);

            this.container = builder.BuildContainer();
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
            return this.container.Get(service, key);
        }

        /// <summary>
        /// Override which uses StyletIoC as the implementation for IoC.GetAll
        /// </summary>
        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return this.container.GetAll(service);
        }

        /// <summary>
        /// Override which uses StyletIoC as the implementation for IoC.BuildUp
        /// </summary>
        protected override void BuildUp(object instance)
        {
            this.container.BuildUp(instance);
        }
    }
}
