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
            // Stitch the IoC shell to us
            IoC.GetInstance = this.GetInstance;
            IoC.GetAllInstances = this.GetAllInstances;
            IoC.BuildUp = this.BuildUp;

            // Use the current SynchronizationContext for the Execute helper
            Execute.Dispatcher = new DispatcherWrapper(Dispatcher.CurrentDispatcher);

            // Add the current assembly to the assemblies list - this will be needed by the IViewManager
            // However it must be done *after* the SynchronizationContext has been set, or we'll try to raise a PropertyChanged notification and fail
            AssemblySource.Assemblies.Clear();
            AssemblySource.Assemblies.AddRange(this.SelectAssemblies());

            this.Configure();

            View.ViewManager = IoC.Get<IViewManager>();

            if (!Execute.InDesignMode)
                this.Launch();
        }

        /// <summary>
        /// Launch the root view
        /// </summary>
        protected virtual void Launch()
        {
            IoC.Get<IWindowManager>().ShowWindow(IoC.Get<TRootViewModel>());
        }

        /// <summary>
        /// Override to configure your IoC container, and anything else
        /// </summary>
        protected virtual void Configure() { }

        /// <summary>
        /// Override this to fetch an implementation of a service from your IoC container. Used by IoC.Get.
        /// </summary>
        /// <param name="service">Service type to fetch an implementation of</param>
        /// <param name="key">String key passed to IoC.Get</param>
        /// <returns>An instance implementing the service</returns>
        protected abstract object GetInstance(Type service, string key = null);

        /// <summary>
        /// Override this to fetch all implementations of a service from your IoC container. Used by IoC.GetAll.
        /// </summary>
        /// <param name="service">Service type to fetch all implementations for</param>
        /// <returns>All instances implementing the service</returns>
        protected abstract IEnumerable<object> GetAllInstances(Type service);

        /// <summary>
        /// Override this to build up an instance using your IoC container. Used by IoC.BuildUp
        /// </summary>
        /// <param name="instance">Instance to build up</param>
        protected abstract void BuildUp(object instance);

        /// <summary>
        /// Initial contents of AssemblySource.Assemblies, defaults to the entry assembly
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<Assembly> SelectAssemblies()
        {
            return new[] { typeof(BootstrapperBase<>).Assembly, this.GetType().Assembly };
        }

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
