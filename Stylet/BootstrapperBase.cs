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
    public abstract class BootstrapperBase<TRootViewModel> where TRootViewModel : class
    {
        /// <summary>
        /// Reference to the current application
        /// </summary>
        protected Application Application { get; private set; }

        /// <summary>
        /// Create a new BootstrapperBase, which automatically starts on application startup
        /// </summary>
        public BootstrapperBase() : this(true) { }

        /// <summary>
        /// Create a new BootstrapperBase, and specify whether to auto-start
        /// </summary>
        /// <param name="autoStart">True to call this.Start() at the end of this constructor</param>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "We give the user the option to not call the virtual method, and call it themselves, if they want/need to")]
        public BootstrapperBase(bool autoStart)
        {
            this.Application = Application.Current;

            // Allows for unit testing
            if (this.Application != null)
            {
                this.Application.Startup += (o, e) =>
                {
                    this.OnApplicationStartup(o, e);
                    if (autoStart)
                        this.Start();
                };

                // Make life nice for the app - they can handle these by overriding Bootstrapper methods, rather than adding event handlers
                this.Application.Exit += OnApplicationExit;

                // Fetch this logger when needed. If we fetch it now, then no-one will have been given the change to enable the LogManager, and we'll get a NullLogger
                this.Application.DispatcherUnhandledException += (o, e) => LogManager.GetLogger(typeof(BootstrapperBase<>)).Error(e.Exception, "Unhandled exception");
                this.Application.DispatcherUnhandledException += OnApplicationUnhandledExecption;
            }
        }

        /// <summary>
        /// Called from the constructor, this does everything necessary to start the application
        /// </summary>
        /// <param name="autoLaunch">True to automatically launch the main window</param>
        protected virtual void Start(bool autoLaunch = true)
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

            this.ConfigureResources();
            this.Configure();

            this.OnStart();

            View.ViewManager = IoC.Get<IViewManager>();

            if (autoLaunch && !Execute.InDesignMode)
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
        /// Add any application resources to the application. Override to add your own, or to avoid Stylet's default resources from being added
        /// </summary>
        protected virtual void ConfigureResources()
        {
            if (this.Application == null)
                return;

            var rc = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Stylet;component/Xaml/StyletResourceDictionary.xaml", UriKind.Absolute) };
            this.Application.Resources.MergedDictionaries.Add(rc);
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
        /// Hook called when the application has started, and Stylet is fully initialised
        /// </summary>
        protected virtual void OnStart() { }

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
