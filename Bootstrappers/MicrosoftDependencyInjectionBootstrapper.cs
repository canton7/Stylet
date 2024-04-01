using Autofac;
using Microsoft.Extensions.DependencyInjection;
using Stylet;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Bootstrappers;

public class MicrosoftDependencyInjectionBootstrapper<TRootViewModel> : BootstrapperBase where TRootViewModel : class
{
    private ServiceProvider serviceProvider;

    private TRootViewModel _rootViewModel;
    protected virtual TRootViewModel RootViewModel => this._rootViewModel ??= (TRootViewModel)this.GetInstance(typeof(TRootViewModel));

    public IServiceProvider ServiceProvider => this.serviceProvider;

    protected override void ConfigureBootstrapper()
    {
        var services = new ServiceCollection();
        this.DefaultConfigureIoC(services);
        this.ConfigureIoC(services);
        this.serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Carries out default configuration of the IoC container. Override if you don't want to do this
    /// </summary>
    protected virtual void DefaultConfigureIoC(IServiceCollection services)
    {
        var viewManagerConfig = new ViewManagerConfig()
        {
            ViewFactory = this.GetInstance,
            ViewAssemblies = new List<Assembly>() { this.GetType().Assembly }
        };

        services.AddSingleton<IViewManager>(new ViewManager(viewManagerConfig));
        services.AddTransient<MessageBoxView>();

        services.AddSingleton<IWindowManagerConfig>(this);
        services.AddSingleton<IWindowManager, WindowManager>();
        services.AddSingleton<IEventAggregator, EventAggregator>();
        services.AddTransient<IMessageBoxViewModel, MessageBoxViewModel>(); // Not singleton!
        // Also need a factory
        services.AddSingleton<Func<IMessageBoxViewModel>>(() => new MessageBoxViewModel());
    }

    /// <summary>
    /// Override to add your own types to the IoC container.
    /// </summary>
    protected virtual void ConfigureIoC(IServiceCollection services) { }

    public override object GetInstance(Type type)
    {
        return this.serviceProvider.GetRequiredService(type);
    }

    protected override void Launch()
    {
        base.DisplayRootView(this.RootViewModel);
    }

    public override void Dispose()
    {
        base.Dispose();

        ScreenExtensions.TryDispose(this._rootViewModel);
        if (this.serviceProvider != null)
            this.serviceProvider.Dispose();
    }
}
