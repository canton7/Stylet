using Ninject;
using Stylet;
using System;

namespace Bootstrappers
{
    public class NinjectBootstrapper<TRootViewModel> : BootstrapperBase<TRootViewModel> where TRootViewModel : class
    {
        private IKernel kernel;

        protected override void ConfigureBootstrapper()
        {
            this.Configure();

            this.kernel = new StandardKernel();
            this.DefaultConfigureIoC(this.kernel);
            this.ConfigureIoC(this.kernel);
        }

        /// <summary>
        /// Override to configure anything that needs configuring
        /// </summary>
        protected virtual void Configure() { }

        /// <summary>
        /// Carries out default configuration of the IoC container. Override if you don't want to do this
        /// </summary>
        protected virtual void DefaultConfigureIoC(IKernel kernel)
        {
            kernel.Bind<IViewManagerConfig>().ToConstant(this);
            kernel.Bind<IViewManager>().To<ViewManager>().InSingletonScope();
            kernel.Bind<IWindowManager>().ToMethod(c => new WindowManager(c.Kernel.Get<IViewManager>(), () => c.Kernel.Get<IMessageBoxViewModel>())).InSingletonScope();
            kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
            kernel.Bind<IMessageBoxViewModel>().To<MessageBoxViewModel>(); // Not singleton!
        }

        /// <summary>
        /// Override to add your own types to the IoC container.
        /// </summary>
        protected virtual void ConfigureIoC(IKernel kernel) { }

        public override object GetInstance(Type type)
        {
            return this.kernel.Get(type);
        }
    }
}
