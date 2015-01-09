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
    public abstract class BootstrapperBase<TRootViewModel> : IBootstrapper, IViewManagerConfig where TRootViewModel : class
    {
        /// <summary>
        /// Gets the current application
        /// </summary>
        public Application Application { get; private set; }

        /// <summary>
        /// Gets or sets assemblies which are used for IoC container auto-binding and searching for Views.
        /// Set this in Configure() if you want to override it
        /// </summary>
        public IList<Assembly> Assemblies { get; protected set; }

        /// <summary>
        /// Gets the command line arguments that were passed to the application from either the command prompt or the desktop.
        /// </summary>
        public string[] Args { get; private set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="BootstrapperBase{TRootViewModel}"/> class
        /// </summary>
        public BootstrapperBase()
        {
            this.Assemblies = new List<Assembly>() { typeof(BootstrapperBase<>).Assembly, this.GetType().Assembly };
        }

        /// <summary>
        /// Called by the ApplicationLoader when this bootstrapper is loaded
        /// </summary>
        /// <param name="application">Application within which Stylet is running</param>
        public void Setup(Application application)
        {
            if (application == null)
                throw new ArgumentNullException("application");

            this.Application = application;

            // Use the current application's dispatcher for Execute
            Execute.Dispatcher = new DispatcherWrapper(this.Application.Dispatcher);

            this.Application.Startup += (o, e) => this.Start(e.Args);
            // Make life nice for the app - they can handle these by overriding Bootstrapper methods, rather than adding event handlers
            this.Application.Exit += (o, e) => this.OnExit(e);

            // Fetch this logger when needed. If we fetch it now, then no-one will have been given the change to enable the LogManager, and we'll get a NullLogger
            this.Application.DispatcherUnhandledException += (o, e) => LogManager.GetLogger(typeof(BootstrapperBase<>)).Error(e.Exception, "Unhandled exception");
            this.Application.DispatcherUnhandledException += (o, e) => this.OnUnhandledExecption(e);
        }

        /// <summary>
        /// Called on Application.Startup, this does everything necessary to start the application
        /// </summary>
        /// <param name="args">Command-line arguments used to start this executable</param>
        public virtual void Start(string[] args)
        {
            // Set this before anything else, so everything can use it
            this.Args = args;

            this.ConfigureBootstrapper();

            View.ViewManager = (IViewManager)this.GetInstance(typeof(IViewManager));

            this.Launch();
        }

        /// <summary>
        /// Launch the root view
        /// </summary>
        protected virtual void Launch()
        {
            var windowManager = (IWindowManager)this.GetInstance(typeof(IWindowManager));
            var rootViewModel = this.GetInstance(typeof(TRootViewModel));
            windowManager.ShowWindow(rootViewModel);
            this.OnStartup();
        }

        /// <summary>
        /// Override to configure your IoC container, and anything else
        /// </summary>
        protected virtual void ConfigureBootstrapper() { }

        /// <summary>
        /// Given a type, use the IoC container to fetch an instance of it
        /// </summary>
        /// <param name="type">Type of instance to fetch</param>
        /// <returns>Fetched instance</returns>
        public abstract object GetInstance(Type type);

        /// <summary>
        /// Hook called on application startup. This occurs once the root view has been displayed
        /// </summary>
        protected virtual void OnStartup() { }

        /// <summary>
        /// Hook called on application exit
        /// </summary>
        /// <param name="e">The exit event data</param>
        protected virtual void OnExit(ExitEventArgs e) { }

        /// <summary>
        /// Hook called on an unhandled exception
        /// </summary>
        /// <param name="e">The event data</param>
        protected virtual void OnUnhandledExecption(DispatcherUnhandledExceptionEventArgs e) { }
    }
}
