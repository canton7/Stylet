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
    public class Bootstrapper<TRootViewModel>
    {
        protected Application Application { get; private set; }

        public Bootstrapper()
        {
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

        protected virtual object GetInstance(Type service, string key = null)
        {
            if (service == typeof(IWindowManager)) service = typeof(WindowManager);
            return Activator.CreateInstance(service);
        }

        protected virtual IEnumerable<object> GetAllInstances(Type service)
        {
            return new[] { Activator.CreateInstance(service) };
        }

        protected virtual void BuildUp(object instance) { }

        protected IEnumerable<Assembly> SelectAssemblies()
        {
            return new[] { Assembly.GetEntryAssembly() };
        }

        protected virtual void OnStartup(object sender, StartupEventArgs e) { }
        protected virtual void OnExit(object sender, EventArgs e) { }
        protected virtual void OnUnhandledExecption(object sender, DispatcherUnhandledExceptionEventArgs e) { }
    }
}
