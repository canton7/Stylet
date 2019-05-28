using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Releasers;
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
        protected virtual object RootViewModel
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
            var viewManagerConfig = new ViewManagerConfig()
            {
                ViewFactory = this.GetInstance,
                ViewAssemblies = new List<Assembly>() { this.GetType().Assembly }
            };

            // Stylet does its own disposal of ViewModels: Castle Windsor shouldn't be doing the same
            // Castle Windsor seems to be ver opinionated on this point, insisting that the container
            // should be responsible for disposing all components. This is at odds with Stylet's approach
            // (and indeed common sense).
#pragma warning disable CS0618 // Type or member is obsolete
            container.Kernel.ReleasePolicy = new NoTrackingReleasePolicy();
#pragma warning restore CS0618 // Type or member is obsolete

            container.Register(
                Component.For<IViewManager>().Instance(new ViewManager(viewManagerConfig)),
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
