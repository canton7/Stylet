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
    // We pretend to be a ResourceDictionary so the user can do:
    // <Application.Resources><ResourceDictionary>
    //     <ResourceDictionary.MergedDictionaries>
    //         <local:Bootstrapper/>        
    //     </ResourceDictionary.MergedDictionaries>
    //  </ResourceDictionary></Application.Resources>
    // rather than:
    // <Application.Resources><ResourceDictionary>
    //     <ResourceDictionary.MergedDictionaries>
    //         <ResourceDictionary>
    //             <local:Bootstrapper x:Key="bootstrapper"/>        
    //         </ResourceDictionary>
    //     </ResourceDictionary.MergedDictionaries>
    //  </ResourceDictionary></Application.Resources>
    // And also so that we can load the Stylet resources
    public abstract class BootstrapperBase<TRootViewModel> : ResourceDictionary
    {
        protected Application Application { get; private set; }

        public BootstrapperBase()
        {
            var rc = new ResourceDictionary() { Source = new Uri("/Stylet;component/StyletResourceDictionary.xaml", UriKind.Relative) };
            this.MergedDictionaries.Add(rc);

            this.Start();
        }

        protected virtual void Start()
        {
            this.Application = Application.Current;
            Execute.SynchronizationContext = SynchronizationContext.Current;

            this.Application.Startup += OnStartup;
            this.Application.Exit += OnExit;
            this.Application.DispatcherUnhandledException += OnUnhandledExecption;

            this.Application.Startup += (o, e) =>
            {
                IoC.Get<IWindowManager>().ShowWindow(IoC.Get<TRootViewModel>());
            };

            AssemblySource.Assemblies.Clear();
            AssemblySource.Assemblies.AddRange(this.SelectAssemblies());

            IoC.GetInstance = this.GetInstance;
            IoC.GetAllInstances = this.GetAllInstances;
            IoC.BuildUp = this.BuildUp;
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
