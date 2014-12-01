using Ninject;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Override to configure your IoC container, and anything else
        /// </summary>
        protected virtual void Configure() { }

        /// <summary>
        /// Carries out default configuration of the IoC container. Override if you don't want to do this
        /// </summary>
        protected virtual void DefaultConfigureIoC(IKernel kernel)
        {
            var viewManager = new ViewManager(this.Assemblies, type => kernel.Get(type));
            kernel.Bind<IViewManager>().ToConstant(viewManager);
            kernel.Bind<IWindowManager>().ToConstant(new WindowManager(viewManager, () => kernel.Get<IMessageBoxViewModel>()));
            kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
            kernel.Bind<IMessageBoxViewModel>().To<MessageBoxViewModel>(); // Not singleton!
        }

        /// <summary>
        /// Override to add your own types to the IoC container.
        /// </summary>
        protected virtual void ConfigureIoC(IKernel kernel) { }

        protected override T GetInstance<T>()
        {
            return this.kernel.Get<T>();
        }
    }
}
