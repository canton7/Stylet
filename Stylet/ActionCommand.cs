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

using Expressions = System.Linq.Expressions;

namespace Stylet
{
    public class ActionCommand : ICommand
    {
        private FrameworkElement subject;
        private string methodName;
        private Func<bool> guardPropertyGetter;
        private MethodInfo targetMethodInfo;

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
            MethodInfo targetMethodInfo = null;
            
            this.guardPropertyGetter = null;
            if (newTarget != null)
            {
                var newTargetType = newTarget.GetType();

                var guardPropertyInfo = newTargetType.GetProperty(this.GuardName);
                if (guardPropertyInfo != null && guardPropertyInfo.PropertyType == typeof(bool))
                {
                    var param = Expressions.Expression.Parameter(typeof(bool), "returnValue");
                    var propertyAccess = Expressions.Expression.Property(param, guardPropertyInfo);
                    this.guardPropertyGetter = Expressions.Expression.Lambda<Func<bool>>(propertyAccess, param).Compile();
                }

                targetMethodInfo = newTargetType.GetMethod(this.methodName);
                if (targetMethodInfo == null)
                    throw new ArgumentException(String.Format("Unable to find method {0} on {1}", this.methodName, this.target.GetType().Name));
            }

            var oldTarget = this.target as INotifyPropertyChanged;
            if (oldTarget != null)
                oldTarget.PropertyChanged -= this.PropertyChangedHandler;

            this.target = newTarget;

            var inpc = newTarget as INotifyPropertyChanged;
            if (this.guardPropertyGetter != null && inpc != null)
                inpc.PropertyChanged += this.PropertyChangedHandler;

            this.targetMethodInfo = targetMethodInfo;

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
            if (this.target == null)
                return false;

            if (this.guardPropertyGetter == null)
                return true;

            return this.guardPropertyGetter();
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            // This is not going to be called very often, so don't bother to generate a delegate, in the way that we do for the method guard
            if (this.target == null)
                throw new ArgumentException("Target not set");

            var parameters = this.targetMethodInfo.GetParameters().Length == 1 ? new[] { parameter } : null;
            this.targetMethodInfo.Invoke(this.target, parameters);
        }
    }
}
