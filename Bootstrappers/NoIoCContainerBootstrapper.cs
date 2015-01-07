using Stylet;
using System;
using System.Collections.Generic;

namespace Bootstrappers
{
    public class NoIoCContainerBootstrapper<TRootViewModel> : BootstrapperBase<TRootViewModel> where TRootViewModel : class
    {
        protected readonly Dictionary<Type, Func<object>> Container = new Dictionary<Type, Func<object>>();

        protected override void ConfigureBootstrapper()
        {
            this.Configure();
            this.DefaultConfigureContainer();
            this.ConfigureContainer();
        }

        /// <summary>
        /// Override to configure anything that needs configuring
        /// </summary>
        protected virtual void Configure() { }

        protected virtual void DefaultConfigureContainer()
        {
            var viewManager = new ViewManager(this);
            this.Container.Add(typeof(IViewManager), () => viewManager);

            var windowManager = new WindowManager(viewManager, () => (IMessageBoxViewModel)this.Container[typeof(IMessageBoxViewModel)]());
            this.Container.Add(typeof(IWindowManager), () => windowManager);

            var eventAggregator = new EventAggregator();
            this.Container.Add(typeof(IEventAggregator), () => eventAggregator);

            this.Container.Add(typeof(IMessageBoxViewModel), () => new MessageBoxViewModel());
        }

        /// <summary>
        /// Use this to add your own types to this.Container. You *MUST* add TRootViewModel
        /// </summary>
        protected virtual void ConfigureContainer() { }

        public override object GetInstance(Type type)
        {
            Func<object> factory;
            if (this.Container.TryGetValue(type, out factory))
                return factory();
            else
                return null;
        }
    }
}
