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
    public abstract class BootstrapperBase<TRootViewModel>
    {
        /// <summary>
        /// Reference to the current application
        /// </summary>
        protected Application Application { get; private set; }

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

            // Use the current SynchronizationContext for the Execute helper
            Execute.SynchronizationContext = SynchronizationContext.Current;

            // Make life nice for the app - they can handle these by overriding Bootstrapper methods, rather than adding event handlers
            this.Application.Startup += OnStartup;
            this.Application.Exit += OnExit;
            this.Application.DispatcherUnhandledException += OnUnhandledExecption;

            // The magic which actually displays
            this.Application.Startup += (o, e) =>
            {
                IoC.Get<IWindowManager>().ShowWindow(IoC.Get<TRootViewModel>());
            };

            // Add the current assembly to the assemblies list - this will be needed by the IViewManager
            AssemblySource.Assemblies.Clear();
            AssemblySource.Assemblies.AddRange(this.SelectAssemblies());

            this.ConfigureResources();
             
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

        protected abstract object GetInstance(Type service, string key = null);
        protected abstract IEnumerable<object> GetAllInstances(Type service);
        protected abstract void BuildUp(object instance);

        protected IEnumerable<Assembly> SelectAssemblies()
        {
            return new[] { Assembly.GetEntryAssembly() };
        }

        protected virtual void OnStartup(object sender, StartupEventArgs e) { }
        protected virtual void OnExit(object sender, EventArgs e) { }
        protected virtual void OnUnhandledExecption(object sender, DispatcherUnhandledExceptionEventArgs e) { }
    }
}
