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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Start must be overridable. It doesn't depend on the type having been constructed")]
        public BootstrapperBase()
        {
            this.Start();
        }

        /// <summary>
        /// Called from the constructor, this does everything necessary to start the application
        /// </summary>
        protected virtual void Start()
        {
            this.Application = Application.Current;

            // Make life nice for the app - they can handle these by overriding Bootstrapper methods, rather than adding event handlers
            this.Application.Startup += OnStartup;
            this.Application.Exit += OnExit;
            this.Application.DispatcherUnhandledException += OnUnhandledExecption;

            // The magic which actually displays
            this.Application.Startup += (o, e) =>
            {
                // Use the current SynchronizationContext for the Execute helper
                Execute.SynchronizationContext = SynchronizationContext.Current;

                IoC.Get<IWindowManager>().ShowWindow(IoC.Get<TRootViewModel>());
            };

            // Add the current assembly to the assemblies list - this will be needed by the IViewManager
            AssemblySource.Assemblies.Clear();
            AssemblySource.Assemblies.AddRange(this.SelectAssemblies());

            this.ConfigureResources();
            this.Configure();
             
            // Stitch the IoC shell to us
            IoC.GetInstance = this.GetInstance;
            IoC.GetAllInstances = this.GetAllInstances;
            IoC.BuildUp = this.BuildUp;
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
