using StructureMap;
using StructureMap.Pipeline;
using Stylet;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;

namespace Bootstrappers
{
    public class StructureMapBootstrapper<TRootViewModel> : BootstrapperBase where TRootViewModel : class
    {
        private IContainer container;

        private object _rootViewModel;
        protected override object RootViewModel
        {
            get { return this._rootViewModel ?? (this._rootViewModel = this.GetInstance(typeof(TRootViewModel))); }
        }

        protected override void ConfigureBootstrapper()
        {
            this.container = new Container(config =>
            {
                this.DefaultConfigureIoC(config);
                this.ConfigureIoC(config);
            });
        }

        /// <summary>
        /// Carries out default configuration of the IoC container. Override if you don't want to do this
        /// </summary>
        protected virtual void DefaultConfigureIoC(ConfigurationExpression config)
        {
            var viewManagerConfig = new ViewManagerConfig()
            {
                ViewAssemblies = new List<Assembly>() { this.GetType().Assembly },
                ViewFactory = this.GetInstance,
            };
            config.For<ViewManagerConfig>().Add(viewManagerConfig);
            config.For<IViewManager>().Add<ViewManager>().LifecycleIs<SingletonLifecycle>();
            config.For<IWindowManagerConfig>().Add(this);
            config.For<IWindowManager>().Add<WindowManager>().LifecycleIs<SingletonLifecycle>();
            config.For<IEventAggregator>().Add<EventAggregator>().LifecycleIs<SingletonLifecycle>();
            config.For<IMessageBoxViewModel>().Add<MessageBoxViewModel>().LifecycleIs<UniquePerRequestLifecycle>();
            config.Scan(x => x.Assembly(this.GetType().Assembly));
        }

        /// <summary>
        /// Override to add your own types to the IoC container.
        /// </summary>
        protected virtual void ConfigureIoC(ConfigurationExpression config) { }

        public override object GetInstance(Type type)
        {
            return this.container.GetInstance(type);
        }

        public override void Dispose()
        {
            this.container.Dispose();
        }
    }
}
