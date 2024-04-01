using Autofac;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Bootstrappers;

public class AutofacBootstrapper<TRootViewModel> : BootstrapperBase where TRootViewModel : class
{
    private IContainer container;

    private TRootViewModel _rootViewModel;
    protected virtual TRootViewModel RootViewModel => this._rootViewModel ??= (TRootViewModel)this.GetInstance(typeof(TRootViewModel));

    protected override void ConfigureBootstrapper()
    {
        var builder = new ContainerBuilder();
        this.DefaultConfigureIoC(builder);
        this.ConfigureIoC(builder);
        this.container = builder.Build();
    }

    /// <summary>
    /// Carries out default configuration of the IoC container. Override if you don't want to do this
    /// </summary>
    protected virtual void DefaultConfigureIoC(ContainerBuilder builder)
    {
        var viewManagerConfig = new ViewManagerConfig()
        {
            ViewFactory = this.GetInstance,
            ViewAssemblies = new List<Assembly>() { this.GetType().Assembly }
        };
        builder.RegisterInstance<IViewManager>(new ViewManager(viewManagerConfig));
        builder.RegisterType<MessageBoxView>();

        builder.RegisterInstance<IWindowManagerConfig>(this).ExternallyOwned();
        builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
        builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();
        builder.RegisterType<MessageBoxViewModel>().As<IMessageBoxViewModel>().ExternallyOwned(); // Not singleton!

        // See https://github.com/canton7/Stylet/discussions/211
        builder.RegisterAssemblyTypes(this.GetType().Assembly).Where(x => !x.Name.Contains("ProcessedByFody")).ExternallyOwned();
    }

    /// <summary>
    /// Override to add your own types to the IoC container.
    /// </summary>
    protected virtual void ConfigureIoC(ContainerBuilder builder) { }

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
