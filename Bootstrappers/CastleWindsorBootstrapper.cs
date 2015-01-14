using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Stylet;
using System;
using System.Windows;

namespace Bootstrappers
{
    public class CastleWindsorBootstrapper<TRootViewModel> : BootstrapperBase<TRootViewModel> where TRootViewModel : class
    {
        private IWindsorContainer container;

        protected override void ConfigureBootstrapper()
        {
            this.Configure();

            this.container = new WindsorContainer();
            this.DefaultConfigureIoC(this.container);
            this.ConfigureIoC(this.container);
        }

        /// <summary>
        /// Override to configure anything that needs configuring
        /// </summary>
        protected virtual void Configure() { }

        /// <summary>
        /// Carries out default configuration of the IoC container. Override if you don't want to do this
        /// </summary>
        protected virtual void DefaultConfigureIoC(IWindsorContainer container)
        {
            container.AddFacility<TypedFactoryFacility>();
            container.Register(
                Component.For<IViewManagerConfig>().Instance(this),
                Component.For<IViewManager>().ImplementedBy<ViewManager>().LifestyleSingleton(),
                Component.For<IWindowManager>().ImplementedBy<WindowManager>().LifestyleSingleton(),
                Component.For<IEventAggregator>().ImplementedBy<EventAggregator>().LifestyleSingleton(),
                Component.For<IMessageBoxViewModel>().ImplementedBy<MessageBoxViewModel>().LifestyleTransient()
            );
            foreach (var assembly in this.Assemblies)
            {
                container.Register(Classes.FromAssembly(assembly).Pick().LifestyleTransient());
            }
        }

        /// <summary>
        /// Override to add your own types to the IoC container.
        /// </summary>
        protected virtual void ConfigureIoC(IWindsorContainer container) { }

        public override object GetInstance(Type type)
        {
            return this.container.Resolve(type);
        }

        protected override void OnExitInternal(ExitEventArgs e)
        {
            this.container.Dispose();
        }
    }
}
