using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Stylet;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;

namespace Bootstrappers
{
    public class CastleWindsorBootstrapper<TRootViewModel> : BootstrapperBase where TRootViewModel : class
    {
        private IWindsorContainer container;

        private object _rootViewModel;
        protected override object RootViewModel
        {
            get { return this._rootViewModel ?? (this._rootViewModel = this.GetInstance(typeof(TRootViewModel))); }
        }

        protected override void ConfigureBootstrapper()
        {
            this.container = new WindsorContainer();
            this.DefaultConfigureIoC(this.container);
            this.ConfigureIoC(this.container);
        }

        /// <summary>
        /// Carries out default configuration of the IoC container. Override if you don't want to do this
        /// </summary>
        protected virtual void DefaultConfigureIoC(IWindsorContainer container)
        {
            container.AddFacility<TypedFactoryFacility>();
            var viewManagerConfig = new ViewManagerConfig()
            {
                ViewAssemblies = new List<Assembly>() { this.GetType().Assembly },
                ViewFactory = this.GetInstance,
            };
            container.Register(
                Component.For<ViewManagerConfig>().Instance(viewManagerConfig),
                Component.For<IWindowManagerConfig>().Instance(this),
                Component.For<IViewManager>().ImplementedBy<ViewManager>().LifestyleSingleton(),
                Component.For<IWindowManager>().ImplementedBy<WindowManager>().LifestyleSingleton(),
                Component.For<IEventAggregator>().ImplementedBy<EventAggregator>().LifestyleSingleton(),
                Component.For<IMessageBoxViewModel>().ImplementedBy<MessageBoxViewModel>().LifestyleTransient()
            );
            container.Register(Classes.FromAssembly(this.GetType().Assembly).Pick().LifestyleTransient());
        }

        /// <summary>
        /// Override to add your own types to the IoC container.
        /// </summary>
        protected virtual void ConfigureIoC(IWindsorContainer container) { }

        public override object GetInstance(Type type)
        {
            return this.container.Resolve(type);
        }

        public override void Dispose()
        {
            this.container.Dispose();
        }
    }
}
