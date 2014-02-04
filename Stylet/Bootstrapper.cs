using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public void Start()
        {
            this.Application = Application.Current;

            this.Application.Startup += OnStartup;
            this.Application.Exit += OnExit;
            this.Application.DispatcherUnhandledException += OnUnhandledExecption;

            this.Application.Startup += (o, e) =>
            {
                new WindowManager().ShowWindow(Activator.CreateInstance(typeof(TRootViewModel)));
            };
        }

        protected virtual void OnStartup(object sender, StartupEventArgs e) { }
        protected virtual void OnExit(object sender, EventArgs e) { }
        protected virtual void OnUnhandledExecption(object sender, DispatcherUnhandledExceptionEventArgs e) { }
    }
}
