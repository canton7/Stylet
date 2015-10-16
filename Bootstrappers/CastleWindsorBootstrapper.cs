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
            var viewManager = new ViewManager(this.GetInstance, new List<Assembly>() { this.GetType().Assembly });
            container.Register(
                Component.For<IViewManager>().Instance(viewManager),
                Component.For<IWindowManagerConfig>().Instance(this),
                Component.For<IMessageBoxViewModel>().ImplementedBy<MessageBoxViewModel>().LifestyleTransient(),
                // For some reason we need to register the delegate separately?
                Component.For<Func<IMessageBoxViewModel>>().Instance(() => new MessageBoxViewModel()),
                Component.For<IWindowManager>().ImplementedBy<WindowManager>().LifestyleSingleton(),
                Component.For<IEventAggregator>().ImplementedBy<EventAggregator>().LifestyleSingleton()
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
