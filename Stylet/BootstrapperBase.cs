using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        public BootstrapperBase()
        {
            // Stitch the IoC shell to us
            IoC.GetInstance = this.GetInstance;
            IoC.GetAllInstances = this.GetAllInstances;
            IoC.BuildUp = this.BuildUp;

            this.Application = Application.Current;

            // Call this before calling our Start method
            this.Application.Startup += this.OnStartup;
            this.Application.Startup += (o, e) => this.Start();

            // Make life nice for the app - they can handle these by overriding Bootstrapper methods, rather than adding event handlers
            this.Application.Exit += OnExit;
            this.Application.DispatcherUnhandledException += OnUnhandledExecption;
        }

        /// <summary>
        /// Called from the constructor, this does everything necessary to start the application
        /// </summary>
        protected virtual void Start()
        {
            // Use the current SynchronizationContext for the Execute helper
            Execute.Dispatcher = new DispatcherWrapper();

            // Add the current assembly to the assemblies list - this will be needed by the IViewManager
            // However it must be done *after* the SynchronizationContext has been set, or we'll try to raise a PropertyChanged notification and fail
            AssemblySource.Assemblies.Clear();
            AssemblySource.Assemblies.AddRange(this.SelectAssemblies());

            this.ConfigureResources();
            this.Configure();

            View.ViewManager = IoC.Get<IViewManager>();
            IoC.Get<IWindowManager>().ShowWindow(IoC.Get<TRootViewModel>());
        }

        protected virtual void ConfigureResources()
        {
            var rc = new ResourceDictionary() { Source = new Uri("pack://application:,,,/Stylet;component/Xaml/StyletResourceDictionary.xaml", UriKind.Absolute) };
            Application.Resources.MergedDictionaries.Add(rc);
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
        protected IEnumerable<Assembly> SelectAssemblies()
        {
            return new[] { Assembly.GetEntryAssembly() };
        }

        /// <summary>
        /// Hook called on application startup
        /// </summary>
        protected virtual void OnStartup(object sender, StartupEventArgs e) { }

        /// <summary>
        /// Hook called on application exit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnExit(object sender, EventArgs e) { }

        /// <summary>
        /// Hook called on an unhandled exception
        /// </summary>
        protected virtual void OnUnhandledExecption(object sender, DispatcherUnhandledExceptionEventArgs e) { }
    }
}
