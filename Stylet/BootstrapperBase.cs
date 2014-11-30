using Stylet.Logging;
using Stylet.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace Stylet
{
    /// <summary>
    /// Bootstrapper to be extended by applications which don't want to use StyletIoC as the IoC container.
    /// </summary>
    /// <typeparam name="TRootViewModel">Type of the root ViewModel. This will be instantiated and displayed</typeparam>
    public abstract class BootstrapperBase<TRootViewModel> : IBootstrapper where TRootViewModel : class
    {
        /// <summary>
        /// Reference to the current application
        /// </summary>
        protected Application Application { get; private set; }

        /// <summary>
        /// Assemblies which are used for IoC container auto-binding and searching for Views.
        /// Set this in Configure() if you want to override it
        /// </summary>
        protected List<Assembly> Assemblies { get; set; }

        /// <summary>
        /// Instantiate a new BootstrapperBase
        /// </summary>
        public BootstrapperBase()
        {
            this.Assemblies = new List<Assembly>() { typeof(BootstrapperBase<>).Assembly, this.GetType().Assembly };
        }

        /// <summary>
        /// Called by the ApplicationLoader when this bootstrapper is loaded
        /// </summary>
        /// <param name="application"></param>
        public void Setup(Application application)
        {
            this.Application = application;

            this.Application.Startup += (o, e) =>
            {
                this.OnApplicationStartup(o, e);
                this.Start();
            };

            // Make life nice for the app - they can handle these by overriding Bootstrapper methods, rather than adding event handlers
            this.Application.Exit += OnApplicationExit;

            // Fetch this logger when needed. If we fetch it now, then no-one will have been given the change to enable the LogManager, and we'll get a NullLogger
            this.Application.DispatcherUnhandledException += (o, e) => LogManager.GetLogger(typeof(BootstrapperBase<>)).Error(e.Exception, "Unhandled exception");
            this.Application.DispatcherUnhandledException += OnApplicationUnhandledExecption;
        }

        /// <summary>
        /// Called on Application.Startup, this does everything necessary to start the application
        /// </summary>
        protected virtual void Start()
        {
            // Use the current SynchronizationContext for the Execute helper
            Execute.Dispatcher = new DispatcherWrapper(Dispatcher.CurrentDispatcher);

            this.ConfigureBootstrapper();

            View.ViewManager = this.GetInstance<IViewManager>();

            if (!Execute.InDesignMode)
                this.Launch();
        }

        /// <summary>
        /// Launch the root view
        /// </summary>
        protected virtual void Launch()
        {
            var windowManager = this.GetInstance<IWindowManager>();
            var rootViewModel = this.GetInstance<TRootViewModel>();
            windowManager.ShowWindow(rootViewModel);
        }

        /// <summary>
        /// Override to configure your IoC container, and anything else
        /// </summary>
        protected virtual void ConfigureBootstrapper() { }

        /// <summary>
        /// Given a type, use the IoC container to fetch an instance of it
        /// </summary>
        /// <typeparam name="T">Instance of type to fetch</typeparam>
        /// <returns>Fetched instance</returns>
        protected abstract T GetInstance<T>();

        /// <summary>
        /// Hook called on application startup. This occurs before Start() is called (if autoStart is true)
        /// </summary>
        protected virtual void OnApplicationStartup(object sender, StartupEventArgs e) { }

        /// <summary>
        /// Hook called on application exit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnApplicationExit(object sender, ExitEventArgs e) { }

        /// <summary>
        /// Hook called on an unhandled exception
        /// </summary>
        protected virtual void OnApplicationUnhandledExecption(object sender, DispatcherUnhandledExceptionEventArgs e) { }
    }
}
