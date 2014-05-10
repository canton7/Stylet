using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StyletIntegrationTests
{
    public class Bootstrapper : Bootstrapper<ShellViewModel>
    {
        protected override void OnUnhandledExecption(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            base.OnUnhandledExecption(sender, e); // Calling this just to trigger some code coverage
            var message = e.Exception.Message;
            if (e.Exception is TargetInvocationException)
                message = e.Exception.InnerException.Message;
            MessageBox.Show(String.Format("Unhandled Exception: {0}", message));
            e.Handled = true;
        }
    }
}
