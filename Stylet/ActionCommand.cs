using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Stylet
{
    public class ActionCommand : ICommand
    {
        private FrameworkElement subject;
        private string methodName;
        private PropertyInfo guardPropertyInfo;

        private object target;

        public ActionCommand(FrameworkElement subject, string methodName)
        {
            this.subject = subject;
            this.methodName = methodName;

            this.UpdateGuardHandler();

            DependencyPropertyDescriptor.FromProperty(View.TargetProperty, typeof(View)).AddValueChanged(this.subject, (o, e) => this.UpdateGuardHandler());
        }

        private string GuardName
        {
            get { return "Can" + this.methodName; }
        }

        private void UpdateGuardHandler()
        {
            var newTarget = View.GetTarget(this.subject);
            this.guardPropertyInfo = null;
            if (newTarget != null)
            {
                var guardPropertyInfo = newTarget.GetType().GetProperty(this.GuardName);
                if (guardPropertyInfo != null && guardPropertyInfo.PropertyType == typeof(bool))
                    this.guardPropertyInfo = guardPropertyInfo;
            }

            var oldTarget = this.target as INotifyPropertyChanged;
            if (oldTarget != null)
                oldTarget.PropertyChanged -= this.PropertyChangedHandler;

            this.target = newTarget;

            var inpc = newTarget as INotifyPropertyChanged;
            if (this.guardPropertyInfo != null && inpc != null)
                inpc.PropertyChanged += this.PropertyChangedHandler;

            this.UpdateCanExecute();
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (String.IsNullOrEmpty(e.PropertyName) || e.PropertyName == this.GuardName)
            {
                this.UpdateCanExecute();
            }
        }

        private void UpdateCanExecute()
        {
            var handler = this.CanExecuteChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            if (this.guardPropertyInfo == null)
                return true;

            return (bool)this.guardPropertyInfo.GetValue(this.target);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (this.target == null)
                throw new Exception("Target not set");

            var methodInfo = this.target.GetType().GetMethod(this.methodName);
            if (methodInfo == null)
                throw new Exception(String.Format("Unable to find method {0} on {1}", this.methodName, this.target.GetType().Name));

            if (methodInfo != null)
            {
                var parameters = methodInfo.GetParameters().Length == 1 ? new[] { parameter } : null;
                methodInfo.Invoke(this.target, parameters);
            }
        }
    }
}
