using Stylet;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Bootstrappers
{
    public abstract class NoIoCContainerBootstrapper : BootstrapperBase
    {
        protected readonly Dictionary<Type, Func<object>> Container = new Dictionary<Type, Func<object>>();

        protected override void ConfigureBootstrapper()
        {
            this.DefaultConfigureContainer();
            this.ConfigureContainer();
        }

        protected abstract object RootViewModel { get; }

        protected virtual void DefaultConfigureContainer()
        {
            var viewManagerConfig = new ViewManagerConfig()
            {
                ViewFactory = this.GetInstance,
                ViewAssemblies = new List<Assembly>() { this.GetType().Assembly }
            };
            var viewManager = new ViewManager(viewManagerConfig);
            this.Container.Add(typeof(IViewManager), () => viewManager);

            var windowManager = new WindowManager(viewManager, () => (IMessageBoxViewModel)this.Container[typeof(IMessageBoxViewModel)](), this);
            this.Container.Add(typeof(IWindowManager), () => windowManager);

            var eventAggregator = new EventAggregator();
            this.Container.Add(typeof(IEventAggregator), () => eventAggregator);

            this.Container.Add(typeof(IMessageBoxViewModel), () => new MessageBoxViewModel());
        }

        /// <summary>
        /// Use this to add your own types to this.Container
        /// </summary>
        protected virtual void ConfigureContainer() { }

        protected override void Launch()
        {
            base.DisplayRootView(this.RootViewModel);
        }

        public override object GetInstance(Type type)
        {
            Func<object> factory;
            if (this.Container.TryGetValue(type, out factory))
                return factory();
            else
                return null;
        }

        public override void Dispose()
        {
            ScreenExtensions.TryDispose(this.RootViewModel);

            base.Dispose();
        }
    }
}
